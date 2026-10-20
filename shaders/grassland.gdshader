shader_type spatial;
render_mode diffuse_lambert, ambient_light_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

// Scene distances are kilometres. The painted colour map stays in world space.
uniform sampler2D grass_canopy_color : source_color, repeat_enable, filter_linear_mipmap;
uniform sampler2D grass_land_noise : repeat_enable, filter_linear_mipmap;
uniform vec4 base_color : source_color = vec4(0.86, 0.96, 1.0, 1.0);
uniform float grass_texture_span = 64.0;
uniform vec4 ambient_color : source_color = vec4(0.70, 0.79, 0.88, 1.0);
uniform float ambient_strength = 0.35;
uniform float direct_strength = 1.0;
uniform float cloud_shadow_strength = 0.42;
uniform vec4 selection_glow : source_color = vec4(0.0);
uniform float grass_wind_strength : hint_range(0.0, 1.0) = 0.75;
uniform float grass_wind_speed : hint_range(0.0, 0.2) = 0.085;
uniform float grass_wind_spacing = 4.8;
uniform float grass_wind_width = 0.85;
uniform float grass_sheen_strength : hint_range(0.0, 0.2) = 0.075;
uniform vec4 fog_color : source_color = vec4(0.62, 0.78, 0.88, 1.0);
uniform float fog_start = 35.0;
uniform float fog_end = 95.0;
uniform float fog_strength = 0.07;

varying vec3 world_position;
varying float cloud_transmission;
varying float atmosphere_transmission;
varying float canopy_sheen;
varying float canopy_exposure;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 p = world_position.xz;
    vec3 ground_normal = normalize((INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
    float top_surface = smoothstep(0.6, 0.95, ground_normal.y);

    // Separate broad painted colour masses from quieter, correctly sized marks.
    // This mapping keeps both the current board and art preview inside one tile.
    vec2 paint_uv = p / grass_texture_span + vec2(0.375);
    vec3 paint = textureLod(grass_canopy_color, paint_uv, 4.0).rgb;
    // Mirrored sampling keeps the detail continuous at repeat edges.
    vec2 detail_uv = 1.0 - abs(fract(p / 6.0 + vec2(0.13, 0.27)) * 2.0 - 1.0);
    vec3 brushwork = texture(grass_canopy_color, detail_uv).rgb;
    float brush_value = dot(brushwork, vec3(0.2126, 0.7152, 0.0722));
    vec3 grass_color = paint * (1.0 + (brush_value - 0.2) * 0.3) * base_color.rgb;
    ALBEDO = grass_color * mix(0.48, 1.0, top_surface);

    vec2 wind = cloud_wind_direction();
    vec2 across_wind = vec2(-wind.y, wind.x);
    vec2 wind_p = vec2(dot(p, wind), dot(p, across_wind));
    float travel = TIME * grass_wind_speed;
    // A low-frequency lookup breaks up the fronts over kilometres, without grain.
    float front_shape = texture(grass_land_noise,
        (wind_p - vec2(travel, 0.0)) * vec2(0.055, 0.11) + vec2(0.61, 0.29)).r;
    float curve = (front_shape - 0.5) * 2.0 + sin(wind_p.y * 0.7) * 0.22;
    float spacing = max(grass_wind_spacing, 0.1);
    float phase = wind_p.x - travel + curve;
    float distance_to_front = mod(phase + spacing * 0.5, spacing) - spacing * 0.5;
    float width = max(grass_wind_width, 0.01) * mix(0.75, 1.25, front_shape);
    // A soft leading edge bends the canopy, followed by a longer recovery.
    float wave = smoothstep(-width, -width * 0.15, distance_to_front)
        * (1.0 - smoothstep(0.0, width * 0.2, distance_to_front));
    // Fronts fade out along their length instead of forming unbroken stripes.
    float envelope = smoothstep(0.2, 0.65, front_shape);
    float gust = wave * envelope * grass_wind_strength * top_surface;
    vec2 canopy_tilt = wind * gust * 0.4
        + across_wind * (front_shape - 0.5) * gust * 0.09;
    vec3 canopy_normal = normalize(ground_normal + vec3(canopy_tilt.x, 0.0, canopy_tilt.y));
    NORMAL = normalize((VIEW_MATRIX * vec4(canopy_normal, 0.0)).xyz);
    ROUGHNESS = 0.68;
    canopy_sheen = grass_sheen_strength * top_surface * (0.5 + gust * 0.5);
    // Aggregate leaf exposure reinforces motion at the tactical camera scale.
    // It affects direct light, so wind never paints a moving albedo texture.
    canopy_exposure = 1.0 + gust * 0.22;

    cloud_transmission = 1.0 - cloud_shadow(world_position) * cloud_shadow_strength;
    float fog_amount = smoothstep(fog_start, fog_end, length(VERTEX)) * fog_strength;
    atmosphere_transmission = 1.0 - fog_amount;
    EMISSION = (ALBEDO * ambient_color.rgb * ambient_strength + selection_glow.rgb)
        * atmosphere_transmission + fog_color.rgb * fog_amount;
}

void light() {
    float facing = max(dot(NORMAL, LIGHT), 0.0);
    vec3 sunlight = LIGHT_COLOR * ATTENUATION * cloud_transmission
        * atmosphere_transmission * direct_strength;
    // Godot multiplies DIFFUSE_LIGHT by ALBEDO after the light processor.
    DIFFUSE_LIGHT += sunlight * facing * canopy_exposure / PI;
    vec3 half_vector = normalize(LIGHT + VIEW);
    // Roughness widens the lobe instead of simply suppressing its intensity.
    float highlight_power = mix(4.0, 32.0, (1.0 - ROUGHNESS) * (1.0 - ROUGHNESS));
    float highlight = pow(max(dot(NORMAL, half_vector), 0.0), highlight_power);
    SPECULAR_LIGHT += sunlight * highlight * canopy_sheen;
}
