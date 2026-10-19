shader_type spatial;
render_mode diffuse_lambert, specular_disabled, ambient_light_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

uniform vec4 base_color : source_color = vec4(0.48, 0.68, 0.28, 1.0);
uniform vec4 ambient_color : source_color = vec4(0.70, 0.79, 0.88, 1.0);
uniform float ambient_strength : hint_range(0.0, 1.0) = 0.32;
uniform float direct_strength : hint_range(0.0, 2.0) = 0.65;
uniform float light_steps : hint_range(2.0, 8.0, 1.0) = 3.0;
uniform float cloud_shadow_strength : hint_range(0.0, 1.0) = 0.52;
uniform float grass_wind_strength : hint_range(0.0, 0.5) = 0.0;
uniform vec2 grass_wind_direction = vec2(0.92, 0.38);
uniform float grass_wind_speed = 3.0;
uniform float grass_wind_spacing = 32.0;
uniform float grass_wind_width = 3.5;
uniform float vertex_ao_strength : hint_range(0.0, 1.0) = 0.0;
uniform vec4 selection_glow : source_color = vec4(0.0);
uniform vec4 fog_color : source_color = vec4(0.62, 0.78, 0.88, 1.0);
uniform float fog_start = 35.0;
uniform float fog_end = 95.0;
uniform float fog_strength : hint_range(0.0, 1.0) = 0.07;

varying vec3 world_position;
varying float cloud_transmission;
varying float atmosphere_transmission;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec3 face_normal = normalize(cross(dFdx(VERTEX), dFdy(VERTEX)));
    NORMAL = face_normal * (dot(face_normal, NORMAL) < 0.0 ? -1.0 : 1.0);
    float height_to_cloud = max(cloud_height - world_position.y, 0.0);
    vec2 cloud_position = world_position.xz + sun_direction.xz
        * height_to_cloud / max(sun_direction.y, 0.05);
    cloud_transmission = 1.0 - cloud_density(cloud_position) * cloud_shadow_strength;
    float fog_amount = smoothstep(fog_start, max(fog_end, fog_start + 0.01), length(VERTEX)) * fog_strength;
    atmosphere_transmission = 1.0 - fog_amount;
    float occlusion = mix(1.0, COLOR.r, vertex_ao_strength);
    ALBEDO = base_color.rgb * occlusion;
    vec2 wind_direction = grass_wind_direction / max(length(grass_wind_direction), 0.001);
    float wind_noise = sin(dot(world_position.xz, vec2(-wind_direction.y, wind_direction.x)) * 0.48
        + TIME * 0.37);
    float phase = dot(world_position.xz, wind_direction) - TIME * grass_wind_speed + wind_noise * 1.6;
    float spacing = max(grass_wind_spacing, 1.0);
    float distance_to_band = abs(mod(phase + spacing * 0.5, spacing) - spacing * 0.5);
    float gust = 1.0 - smoothstep(0.0, max(grass_wind_width, 0.01), distance_to_band);
    float grass_surface = smoothstep(0.6, 0.95, (INV_VIEW_MATRIX * vec4(NORMAL, 0.0)).y);
    ALBEDO = mix(ALBEDO, ALBEDO * vec3(1.3, 1.18, 0.88),
        gust * grass_wind_strength * grass_surface);
    ROUGHNESS = 1.0;
    EMISSION = (ALBEDO * ambient_color.rgb * ambient_strength + selection_glow.rgb)
        * atmosphere_transmission + fog_color.rgb * fog_amount;
}

void light() {
    float facing = max(dot(NORMAL, LIGHT), 0.0);
    float band = floor(facing * light_steps + 0.5) / light_steps;
    DIFFUSE_LIGHT += ALBEDO * LIGHT_COLOR * band * ATTENUATION
        * direct_strength * cloud_transmission * atmosphere_transmission;
}