shader_type canvas_item;
render_mode blend_mix;

uniform float wind_wave_spacing : hint_range(20.0, 200.0, 1.0) = 70.0;
uniform float wind_speed : hint_range(0.1, 3.0, 0.1) = 1.0;
uniform vec2 wind_direction = vec2(-1.0, 1.0);
uniform float wind_divergence : hint_range(0.0, 45.0, 1.0) = 14.0;
uniform vec4 overlay_color : source_color = vec4(0.48, 0.66, 0.40, 1.0);
uniform bool enable_dynamic = true;

varying vec2 world_position;

float hash(vec2 value) {
    return fract(sin(dot(value, vec2(127.1, 311.7))) * 43758.5453123);
}

float value_noise(vec2 value) {
    vec2 cell = floor(value);
    vec2 local = fract(value);
    local = local * local * (3.0 - 2.0 * local);

    return mix(
        mix(hash(cell), hash(cell + vec2(1.0, 0.0)), local.x),
        mix(hash(cell + vec2(0.0, 1.0)), hash(cell + vec2(1.0, 1.0)), local.x),
        local.y
    );
}

vec2 rotate(vec2 direction, float angle) {
    float cosine = cos(angle);
    float sine = sin(angle);
    return mat2(vec2(cosine, sine), vec2(-sine, cosine)) * direction;
}

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy;
}

void fragment() {
    if (!enable_dynamic) {
        COLOR = vec4(0.0, 0.0, 0.0, 0.0);
    } else {
        vec2 normalized_wind_direction = normalize(wind_direction);
        float divergence_radians = wind_divergence * 0.01745329252;
        float wind_time = TIME * wind_speed;
        float noise_scale = 1.0 / wind_wave_spacing;
        vec2 wind_offset1 = wind_time * rotate(normalized_wind_direction, divergence_radians) * 0.9;
        vec2 wind_offset2 = wind_time * rotate(normalized_wind_direction, -divergence_radians) * 0.76;
        float wind_noise1 = value_noise(world_position * noise_scale - wind_offset1);
        float wind_noise2 = value_noise(world_position * noise_scale * 0.8 - wind_offset2);
        float wind_shape = smoothstep(0.34, 0.58, wind_noise1 * wind_noise2);

        COLOR = vec4(overlay_color.rgb, wind_shape * 0.22);
    }
}
