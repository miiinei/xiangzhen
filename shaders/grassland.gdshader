shader_type spatial;
render_mode diffuse_lambert, ambient_light_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

// One shared painting, sampled at a fixed kilometre scale across every grass hexagon.
// Local painted relief stays in the artwork; the board geometry stays level.
uniform sampler2D grass_canopy_color : source_color, repeat_disable, filter_linear_mipmap;
uniform sampler2D grass_land_noise : repeat_enable, filter_linear_mipmap;
uniform vec2 grass_texture_origin = vec2(16.75, 14.25);
uniform vec2 grass_texture_span = vec2(12.0, 8.0);
uniform vec4 base_color : source_color = vec4(1.0);
uniform vec4 ambient_color : source_color = vec4(0.72, 0.82, 1.0, 1.0);
uniform float ambient_strength = 0.22;
uniform float direct_strength = 1.0;
uniform float cloud_shadow_strength = 0.44;
uniform vec4 selection_glow : source_color = vec4(0.0);
uniform float grass_wind_strength : hint_range(0.0, 1.0) = 0.75;
uniform float grass_wind_speed : hint_range(0.0, 0.2) = 0.085;
uniform float grass_sheen_strength : hint_range(0.0, 0.1) = 0.025;
uniform vec4 fog_color : source_color = vec4(0.62, 0.78, 0.88, 1.0);
uniform float fog_start = 35.0;
uniform float fog_end = 95.0;
uniform float fog_strength = 0.07;

varying vec3 world_position;
varying vec3 rest_leaf_normal;
varying vec3 bent_leaf_normal;
varying float bent_fraction;
varying float water_fraction;
varying float canopy_sheen;
varying float cloud_transmission;
varying float atmosphere_transmission;

vec2 landscape_patch_center(vec2 cell) {
    // Translate crops only: keep the painting's camera and light orientation.
    vec2 seed = vec2(dot(cell, vec2(127.1, 311.7)),
        dot(cell, vec2(269.5, 183.3)));
    return vec2(0.25) + fract(sin(seed) * 43758.5453 + vec2(0.5)) * 0.5;
}

vec3 sample_landscape(vec2 projected) {
    // The painting is not seamless. Overlap bounded crops at one scale rather
    // than stretching it over the board or enlarging it again at the perimeter.
    vec2 painting_uv = projected / grass_texture_span;
    vec2 patch_position = painting_uv * 4.0;
    vec2 cell = floor(patch_position);
    vec2 local = fract(patch_position);
    vec2 blend = smoothstep(vec2(0.35), vec2(0.65), local);
    // Continuous derivatives prevent mip seams at changes of crop index.
    vec2 dx = dFdx(painting_uv);
    vec2 dy = dFdy(painting_uv);
    vec3 a = textureGrad(grass_canopy_color,
        landscape_patch_center(cell) + local * 0.25, dx, dy).rgb;
    vec3 b = textureGrad(grass_canopy_color,
        landscape_patch_center(cell + vec2(1.0, 0.0))
        + (local - vec2(1.0, 0.0)) * 0.25, dx, dy).rgb;
    vec3 c = textureGrad(grass_canopy_color,
        landscape_patch_center(cell + vec2(0.0, 1.0))
        + (local - vec2(0.0, 1.0)) * 0.25, dx, dy).rgb;
    vec3 d = textureGrad(grass_canopy_color,
        landscape_patch_center(cell + vec2(1.0))
        + (local - vec2(1.0)) * 0.25, dx, dy).rgb;
    return mix(mix(a, b, blend.x), mix(c, d, blend.x), blend.y);
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 p = world_position.xz;
    vec3 ground_normal = normalize((INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
    float top_surface = smoothstep(0.6, 0.95, ground_normal.y);
    // The artwork retains the approved oblique view. Undo the fixed camera's
    // 45-degree heading and 35.264-degree pitch in its world-space mapping.
    // This is anchored to the ground, independent of camera pan and zoom.
    vec2 offset = p - grass_texture_origin;
    vec2 projected = vec2(offset.x - offset.y,
        (offset.x + offset.y) * 0.5773502692) * 0.7071067812;
    vec3 paint = sample_landscape(projected);
    ALBEDO = paint * base_color.rgb * mix(0.42, 1.0, top_surface);

    // Visual material response only, not gameplay water or height data.
    water_fraction = smoothstep(0.006, 0.055, paint.b - paint.g) * top_surface;
    float green_cover = smoothstep(0.018, 0.09,
        paint.g - (paint.r + paint.b) * 0.5) * (1.0 - water_fraction);
    float value = dot(paint, vec3(0.2126, 0.7152, 0.0722));
    float dense_cover = 1.0 - smoothstep(0.10, 0.38, value);

    vec2 wind = cloud_wind_direction();
    vec2 across = vec2(-wind.y, wind.x);
    vec2 wind_p = vec2(dot(p, wind), dot(p, across));
    // Advected gust patches replace periodic ridges. Native TIME is independent
    // of the existing cloud cache's 20 Hz clock.
    float travelling = texture(grass_land_noise,
        (wind_p - vec2(TIME * grass_wind_speed, 0.0)) * vec2(0.8, 0.25)
        + vec2(0.61, 0.29)).r;
    float local_cover = texture(grass_land_noise,
        p * vec2(1.7, 1.1) + vec2(0.23, 0.71)).r;
    float gust = smoothstep(0.30, 0.68, travelling);
    float response = green_cover * mix(0.35, 1.0, dense_cover)
        * mix(0.55, 1.0, local_cover) * top_surface;
    bent_fraction = gust * response * grass_wind_strength;
    vec2 rest_tilt = across * (local_cover - 0.5) * 0.12;
    vec2 bent_tilt = wind * mix(0.45, 0.7, dense_cover) + rest_tilt;
    rest_leaf_normal = normalize((VIEW_MATRIX * vec4(normalize(ground_normal
        + vec3(rest_tilt.x, 0.0, rest_tilt.y)), 0.0)).xyz);
    bent_leaf_normal = normalize((VIEW_MATRIX * vec4(normalize(ground_normal
        + vec3(bent_tilt.x, 0.0, bent_tilt.y)), 0.0)).xyz);
    canopy_sheen = grass_sheen_strength * green_cover * top_surface;
    ROUGHNESS = mix(0.90, 0.38, water_fraction);

    cloud_transmission = 1.0 - cloud_shadow(world_position) * cloud_shadow_strength;
    float fog_amount = smoothstep(fog_start, fog_end, length(VERTEX)) * fog_strength;
    atmosphere_transmission = 1.0 - fog_amount;
    EMISSION = (ALBEDO * ambient_color.rgb * ambient_strength + selection_glow.rgb)
        * atmosphere_transmission + fog_color.rgb * fog_amount;
}

void light() {
    vec3 sunlight = LIGHT_COLOR * ATTENUATION * cloud_transmission
        * atmosphere_transmission * direct_strength;
    // Mix upright/bent leaf populations without tilting the terrain or moving
    // the painted puddles, mineral marks and vegetation distribution.
    float upright = max(dot(normalize(rest_leaf_normal), LIGHT), 0.0);
    float bent = max(dot(normalize(bent_leaf_normal), LIGHT), 0.0);
    DIFFUSE_LIGHT += sunlight * mix(upright, bent, bent_fraction) / PI;
    vec3 half_vector = normalize(LIGHT + VIEW);
    float upright_sheen = pow(max(dot(normalize(rest_leaf_normal), half_vector), 0.0), 14.0);
    float bent_sheen = pow(max(dot(normalize(bent_leaf_normal), half_vector), 0.0), 14.0);
    float grass_highlight = mix(upright_sheen, bent_sheen, bent_fraction) * canopy_sheen;
    float water_highlight = pow(max(dot(NORMAL, half_vector), 0.0), 64.0)
        * water_fraction * 0.14;
    SPECULAR_LIGHT += sunlight * (grass_highlight + water_highlight);
}
