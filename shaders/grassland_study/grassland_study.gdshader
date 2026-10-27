shader_type spatial;
render_mode diffuse_lambert, ambient_light_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

uniform sampler2D grass_canopy_color : source_color, repeat_disable, filter_linear_mipmap;
uniform sampler2D grass_fine_marks : source_color, repeat_enable, filter_linear_mipmap;
uniform sampler2D grass_land_noise : repeat_enable, filter_linear_mipmap;
// Data maps are linear. Normals use world XYZ, not tangent-space normal encoding.
uniform sampler2D vegetation_field : repeat_disable, filter_linear_mipmap;
uniform sampler2D surface_normals : repeat_disable, filter_linear_mipmap;
uniform float terrain_span = 40.0;
uniform vec2 terrain_origin = vec2(16.5, 14.5);
uniform float wind_time = 0.0;
uniform float wind_strength : hint_range(0.0, 1.0) = 0.85;
uniform float wind_speed = 0.085;
uniform float cloud_shadow_strength = 0.42;
uniform float sheen_strength : hint_range(0.0, 0.1) = 0.035;
uniform vec4 short_grass_color : source_color = vec4(0.40, 0.55, 0.18, 1.0);
uniform vec4 tall_grass_color : source_color = vec4(0.15, 0.34, 0.18, 1.0);
uniform vec4 ambient_color : source_color = vec4(0.70, 0.79, 0.88, 1.0);
uniform float ambient_strength = 0.28;

varying vec3 world_position;
varying vec3 rest_leaf_normal;
varying vec3 bent_leaf_normal;
varying float bent_fraction;
varying float canopy_sheen;
varying float cloud_transmission;

vec2 mirrored(vec2 uv) {
    return 1.0 - abs(fract(uv) * 2.0 - 1.0);
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 p = world_position.xz - terrain_origin;
    vec2 field_uv = p / terrain_span + vec2(0.5);
    vec4 vegetation = texture(vegetation_field, field_uv);
    vec3 ground_normal = normalize(texture(surface_normals, field_uv).rgb * 2.0 - 1.0);
    NORMAL = normalize((VIEW_MATRIX * vec4(ground_normal, 0.0)).xyz);

    // The painted layout follows the same swale as the vegetation field.
    vec3 palette = mix(short_grass_color.rgb, tall_grass_color.rgb,
        smoothstep(0.25, 0.85, vegetation.r));
    vec3 paint = texture(grass_canopy_color, field_uv).rgb;
    vec3 quiet_paint = textureLod(grass_canopy_color, field_uv, 2.0).rgb;
    float detailed_cover = smoothstep(0.45, 0.82, vegetation.r);
    paint = mix(quiet_paint, paint, mix(0.3, 1.0, detailed_cover));
    ALBEDO = mix(palette, paint, 0.74);
    vec3 marks = texture(grass_fine_marks, mirrored(p / 2.4 + vec2(0.13, 0.27))).rgb;
    float marks_value = dot(marks, vec3(0.2126, 0.7152, 0.0722));
    ALBEDO *= 1.0 + clamp((marks_value - 0.22) * 0.65, -0.06, 0.06) * detailed_cover;

    vec2 wind = cloud_wind_direction();
    vec2 across = vec2(-wind.y, wind.x);
    vec2 wind_p = vec2(dot(p, wind), dot(p, across));
    vec2 moving = wind_p - vec2(wind_time * wind_speed, 0.0);
    float front_shape = texture(grass_land_noise,
        moving * vec2(0.13, 0.23) + vec2(0.61, 0.29)).r;
    float lag = vegetation.b * 0.28 + vegetation.g * 0.20;
    float phase = (moving.x + sin(wind_p.y * 0.67) * 0.45
        + (front_shape - 0.5) * 1.8 - lag) * 2.35;
    float front = pow(0.5 + 0.5 * sin(phase), 3.0);
    float gust = front * mix(0.45, 1.0, smoothstep(0.2, 0.8, front_shape));
    // Short turf barely responds; tall cover bends in spatially separated patches.
    float response = smoothstep(0.18, 0.72, vegetation.g) * mix(0.55, 1.0, vegetation.a);
    bent_fraction = clamp(gust * response * wind_strength, 0.0, 1.0);
    vec2 rest_tilt = across * (vegetation.b - 0.5) * 0.24;
    vec2 bent_tilt = wind * mix(1.15, 1.75, vegetation.g) + rest_tilt * 0.3;
    rest_leaf_normal = normalize((VIEW_MATRIX * vec4(normalize(ground_normal
        + vec3(rest_tilt.x, 0.0, rest_tilt.y)), 0.0)).xyz);
    bent_leaf_normal = normalize((VIEW_MATRIX * vec4(normalize(ground_normal
        + vec3(bent_tilt.x, 0.0, bent_tilt.y)), 0.0)).xyz);
    canopy_sheen = sheen_strength * vegetation.r * mix(0.35, 1.0, vegetation.a);
    ROUGHNESS = 0.88;

    cloud_transmission = 1.0 - cloud_shadow(world_position) * cloud_shadow_strength;
    EMISSION = ALBEDO * ambient_color.rgb * ambient_strength;
}

void light() {
    vec3 sunlight = LIGHT_COLOR * ATTENUATION * cloud_transmission;
    // A restrained contrast curve makes gentle slopes legible under the high sun.
    float upright = pow(max(dot(normalize(rest_leaf_normal), LIGHT), 0.0), 1.5);
    float bent = pow(max(dot(normalize(bent_leaf_normal), LIGHT), 0.0), 1.5);
    // Mix two canopy populations. Never tilt the terrain or amplify a moving ridge.
    DIFFUSE_LIGHT += sunlight * mix(upright, bent, bent_fraction) / PI;
    vec3 half_vector = normalize(LIGHT + VIEW);
    float upright_sheen = pow(max(dot(normalize(rest_leaf_normal), half_vector), 0.0), 14.0);
    float bent_sheen = pow(max(dot(normalize(bent_leaf_normal), half_vector), 0.0), 14.0);
    SPECULAR_LIGHT += sunlight * mix(upright_sheen, bent_sheen, bent_fraction)
        * canopy_sheen * vec3(1.0, 0.96, 0.78);
}
