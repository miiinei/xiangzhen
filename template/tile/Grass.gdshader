shader_type canvas_item;

uniform float wind_wave_spacing : hint_range(20.0, 200.0, 1.0) = 70.0;
uniform float wind_speed : hint_range(0.1, 3.0, 0.1) = 1.0;
uniform float pixel_size : hint_range(1.0, 8.0, 1.0) = 3.0;
uniform float jitter_frequency : hint_range(0.5, 20.0, 0.5) = 5.0;
uniform float jitter_block_size : hint_range(1.0, 16.0, 1.0) = 3.0;
uniform float mottled_patch_size : hint_range(8.0, 64.0, 1.0) = 24.0;
uniform float mottled_softness : hint_range(0.0, 3.0, 0.05) = 0.35;
uniform float mottled_invasion_frequency : hint_range(0.05, 6.0, 0.05) = 1.5;
uniform float mottled_invasion_depth : hint_range(0.0, 32.0, 0.5) = 8.0;
uniform bool use_mottled_background = true;
uniform vec2 wind_direction = vec2(-1.0, 1.0);
uniform float wind_divergence : hint_range(0.0, 45.0, 1.0) = 14.0;
uniform vec4 base_color : source_color = vec4(0.24, 0.37, 0.25, 1.0);
uniform vec4 dark_color : source_color = vec4(0.18, 0.30, 0.20, 1.0);
uniform vec4 light_color : source_color = vec4(0.30, 0.44, 0.29, 1.0);
uniform bool enable_dynamic = true;

varying vec2 world_position;

float hash(vec2 value) {
    return fract(sin(dot(value, vec2(127.1, 311.7))) * 43758.5453123);
}

vec2 hash2(vec2 value) {
    return vec2(
        hash(value + vec2(17.0, 43.0)),
        hash(value + vec2(71.0, 29.0))
    );
}

float cellular_noise(vec2 value) {
    vec2 cell = floor(value);
    vec2 local = fract(value);
    float nearest_distance = 8.0;
    float nearest_value = 0.0;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            vec2 offset = vec2(float(x), float(y));
            vec2 candidate_cell = cell + offset;
            vec2 point = offset + hash2(candidate_cell);
            float distance_to_point = length(local - point);
            if (distance_to_point < nearest_distance)
            {
                nearest_distance = distance_to_point;
                nearest_value = hash(candidate_cell + vec2(103.0, 59.0));
            }
        }
    }

    return nearest_value;
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

vec3 cellular_mottled_color(vec2 pixel_position) {
    vec2 patch_position = pixel_position / mottled_patch_size;
    vec2 invasion_noise = vec2(
        value_noise(patch_position * mottled_invasion_frequency + vec2(31.0, 83.0)),
        value_noise(patch_position * mottled_invasion_frequency + vec2(97.0, 19.0))
    ) - 0.5;
    vec2 fine_invasion = vec2(
        value_noise(patch_position * mottled_invasion_frequency * 2.7 + vec2(13.0, 47.0)),
        value_noise(patch_position * mottled_invasion_frequency * 2.7 + vec2(71.0, 29.0))
    ) - 0.5;
    float patch_value = cellular_noise(
        patch_position
        + (invasion_noise + fine_invasion * 0.45) * (mottled_invasion_depth / mottled_patch_size) * mottled_softness
    );

    if (patch_value < 0.3333) {
        return dark_color.rgb;
    }
    if (patch_value < 0.6667) {
        return base_color.rgb;
    }
    return light_color.rgb;
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
    vec2 pixel_position = floor(world_position / pixel_size) * pixel_size;
    vec3 mottled_color = cellular_mottled_color(pixel_position);
    mottled_color = use_mottled_background ? mottled_color : base_color.rgb;

    if (!enable_dynamic) {
        COLOR = vec4(mottled_color, 1.0);
    } else {
        // Two diverging noise fields avoid the regular bands produced by one sine wave.
        vec2 normalized_wind_direction = normalize(wind_direction);
        float divergence_radians = wind_divergence * 0.01745329252;
        float wind_time = TIME * wind_speed;
        float noise_scale = 1.0 / wind_wave_spacing;
        vec2 wind_offset1 = wind_time * rotate(normalized_wind_direction, divergence_radians) * 0.9;
        vec2 wind_offset2 = wind_time * rotate(normalized_wind_direction, -divergence_radians) * 0.76;
        float wind_noise1 = value_noise(pixel_position * noise_scale - wind_offset1);
        float wind_noise2 = value_noise(pixel_position * noise_scale * 0.8 - wind_offset2);
        float wind_influence = smoothstep(0.28, 0.58, wind_noise1 * wind_noise2);

        vec2 jitter_position = floor(world_position / jitter_block_size) * jitter_block_size;
        vec2 jitter_cell = floor(jitter_position / jitter_block_size);
        float jitter_phase = hash(jitter_cell + vec2(53.0, 19.0));
        float jitter_frame = floor(TIME * jitter_frequency + jitter_phase);
        float jitter_chance = mix(0.025, 0.22, wind_influence);
        float jitter = step(1.0 - jitter_chance, hash(jitter_cell + vec2(jitter_frame, jitter_frame * 0.37)));
        float jitter_color = hash(jitter_cell + vec2(jitter_frame * 0.13, 91.0));
        vec3 grass_color = mix(mottled_color, jitter_color > 0.5 ? light_color.rgb : dark_color.rgb, jitter);

        COLOR = vec4(grass_color, 1.0);
    }
}
