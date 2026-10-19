uniform sampler2D cloud_noise : repeat_enable, filter_linear;
uniform float cloud_scale : hint_range(0.005, 0.2) = 0.065;
uniform vec2 cloud_velocity = vec2(0.22, 0.09);
uniform float cloud_warp : hint_range(0.0, 1.0) = 0.62;
uniform float cloud_coverage : hint_range(0.0, 1.0) = 0.25;
uniform float cloud_softness : hint_range(0.01, 0.4) = 0.035;
uniform float cloud_height = 7.0;
uniform vec3 sun_direction = vec3(-0.287, 0.819, 0.497);

vec2 cloud_hash(vec2 cell) {
    return fract(sin(vec2(dot(cell, vec2(127.1, 311.7)),
        dot(cell, vec2(269.5, 183.3)))) * 43758.5453);
}

float cloud_density(vec2 world_position) {
    vec2 coordinates = (world_position - TIME * cloud_velocity) * cloud_scale;
    vec2 cell = floor(coordinates);
    float density = 0.0;
    float distortion = texture(cloud_noise, coordinates * 0.37).r - 0.5;
    for (int row = -1; row <= 1; row++) {
        for (int column = -1; column <= 1; column++) {
            vec2 neighbor = cell + vec2(float(column), float(row));
            vec2 random_value = cloud_hash(neighbor);
            vec2 center = neighbor + vec2(0.5) + (random_value - 0.5) * 0.24;
            vec2 offset = coordinates - center;
            float radius = mix(0.19, 0.27, random_value.y);
            float outline = min(length(offset / vec2(1.35, 0.8)) - radius,
                min(length(offset - vec2(-0.14, 0.02)) - radius * 0.8,
                    length(offset - vec2(0.09, 0.10)) - radius * 0.85));
            float enabled = step(random_value.x, cloud_coverage);
            density = max(density, enabled * (1.0 - smoothstep(-cloud_softness,
                cloud_softness, outline + distortion * cloud_warp)));
        }
    }
    return density;
}