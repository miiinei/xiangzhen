// Density shaping and light integration reference the MIT cloud demos listed in
// CLOUDS_LICENSES.md. Noise textures are generated locally by Godot.
uniform sampler2D cloud_noise : repeat_enable, filter_linear;
uniform sampler3D cloud_volume_noise : repeat_enable, filter_linear;
uniform sampler3D cloud_base_noise : repeat_enable, filter_linear;
uniform vec3 cloud_weather_origin = vec3(0.0);
uniform float cloud_scale : hint_range(0.005, 0.2) = 0.032;
// Keep the existing grass wind parameter as the shared prevailing direction.
// Cloud drift is slower than the ground gust pattern's travel speed.
uniform vec2 grass_wind_direction = vec2(0.92, 0.38);
uniform float cloud_speed : hint_range(0.0, 1.0) = 0.13;
uniform float cloud_evolution_speed : hint_range(0.0, 0.2) = 0.06;
uniform float cloud_warp : hint_range(0.0, 1.0) = 0.7;
uniform float cloud_coverage : hint_range(0.0, 1.0) = 0.5;
uniform float cloud_softness : hint_range(0.01, 0.4) = 0.26;
uniform float cloud_density_softness : hint_range(0.01, 0.4) = 0.09;
uniform float cloud_height = 7.0;
uniform float cloud_thickness : hint_range(0.5, 8.0) = 3.2;
uniform float cloud_shadow_softness : hint_range(0.0, 3.0) = 0.65;
uniform vec3 sun_direction = vec3(-0.287, 0.819, 0.497);

vec2 cloud_wind_direction() {
    return grass_wind_direction / max(length(grass_wind_direction), 0.001);
}

vec2 cloud_coordinates(vec2 world_position) {
    return (world_position - TIME * cloud_wind_direction() * cloud_speed) * cloud_scale
        + cloud_weather_origin.xz;
}

// Continuous weather has no placement cells or individual cloud outlines.
// Broad gaps, broken patches and stretched filaments share one warped field.
vec2 cloud_weather(vec2 coordinates) {
    float evolution = TIME * cloud_evolution_speed;
    vec2 warp = vec2(
        texture(cloud_noise, coordinates * 0.9 + vec2(0.17, 0.63)
            + evolution * vec2(0.045, -0.03)).r,
        texture(cloud_noise, coordinates * 0.9 + vec2(0.71, 0.29)
            + evolution * vec2(-0.025, 0.04)).r
    ) - vec2(0.5);
    vec2 p = coordinates + warp * cloud_warp * 0.5;
    // Different rates deform the field internally instead of translating a
    // frozen shape. The large weather pattern evolves much more slowly.
    float broad = texture(cloud_noise, p * 0.3 + vec2(0.31, 0.19)
        + evolution * vec2(0.008, 0.004)).r;
    float broken = texture(cloud_noise, p * 1.1 + vec2(0.73, 0.47)
        + evolution * vec2(-0.06, 0.035)).r;
    float field = broad * 0.72 + broken * 0.28;
    float threshold = mix(0.69, 0.35, cloud_coverage);
    float envelope = smoothstep(threshold - cloud_softness * 0.25,
        threshold + cloud_softness * 0.6, field);
    float veil = smoothstep(threshold - 0.14, threshold + cloud_softness * 0.25, field);

    vec2 wind = cloud_wind_direction();
    vec2 across_wind = vec2(-wind.y, wind.x);
    vec2 flowing = vec2(dot(p, wind) * 0.65, dot(p, across_wind) * 2.0);
    flowing += warp * cloud_warp * 0.7;
    float strands = texture(cloud_noise, flowing + vec2(0.41, 0.23)
        + evolution * vec2(0.04, -0.12)).r * 0.75
        + texture(cloud_noise, flowing * 2.2 + vec2(0.67, 0.89)
            + evolution * vec2(-0.09, 0.16)).r * 0.25;
    float filaments = smoothstep(0.3, 0.75, strands);
    return vec2(envelope, veil * filaments * (1.0 - envelope * 0.5));
}

// Rounded positive part extends the density gently into the eroded edge,
// keeping the dense core intact while avoiding a sharp zero-density boundary.
float cloud_soft_density(float shape) {
    float width = max(cloud_density_softness, 0.001);
    float blend = max(width - abs(shape), 0.0) / width;
    return max(shape, 0.0) + blend * blend * width * 0.25;
}

// This full world-space cloud is independent of the camera. Only its visible
// rendering is thinned nearby; ground shadows always sample the full volume.
float cloud_density(vec3 world_position, float detail_contrast) {
    float altitude = (world_position.y - cloud_height) / cloud_thickness;
    if (altitude <= 0.0 || altitude >= 1.0) {
        return 0.0;
    }
    vec2 coordinates = cloud_coordinates(world_position.xz);
    vec2 weather = cloud_weather(coordinates);
    if (max(weather.x, weather.y) < 0.005) {
        return 0.0;
    }
    // Weather only controls horizontal coverage. The cloud's bulk is sampled
    // in isotropic 3D coordinates, rather than extruding a 2D height field.
    float profile = pow(sin(altitude * 3.14159265), 0.75);
    float wisp_profile = exp(-pow((altitude - 0.62) * 9.0, 2.0));
    vec3 p = vec3(coordinates.x, (world_position.y - cloud_height) * cloud_scale,
        coordinates.y) * 2.8 + vec3(0.0, cloud_weather_origin.y, 0.0);
    float evolution = TIME * cloud_evolution_speed;
    float coherent = texture(cloud_base_noise, p * 0.65 + vec3(0.17, 0.31, 0.61)
        + evolution * vec3(0.04, 0.12, -0.06)).r;
    float billows = texture(cloud_volume_noise, p + vec3(0.41, 0.73, 0.19)
        + evolution * vec3(-0.035, 0.16, 0.05)).r;
    billows = smoothstep(0.12, 0.85, billows);
    float erosion = texture(cloud_volume_noise, p * 3.0 + vec3(0.53, 0.13, 0.29)
        + evolution * vec3(-0.12, -0.27, 0.13)).r;
    // Distant rendering suppresses small density contrasts; receiver shadows
    // always sample the complete field with detail_contrast = 1.
    erosion = mix(0.6, erosion, detail_contrast);
    float base_shape = (coherent * 0.55 + billows * 0.45) * profile;
    float threshold = mix(0.78, 0.24, weather.x);
    float edge_erosion = (1.0 - base_shape) * (1.0 - base_shape)
        * (1.0 - erosion) * 0.12;
    float body_shape = (base_shape - threshold - edge_erosion) / (1.0 - threshold);
    float body_presence = smoothstep(0.0, 0.08, weather.x)
        * smoothstep(0.0, 0.08, profile);
    float body = cloud_soft_density(body_shape) * body_presence * 2.8;
    float wisps = weather.y * wisp_profile * mix(0.65, 1.0, erosion) * 0.18;
    return body + wisps;
}

float cloud_shadow(vec3 receiver_position) {
    vec3 sunlight = normalize(sun_direction);
    if (receiver_position.y >= cloud_height + cloud_thickness) {
        return 0.0;
    }
    float start_height = max(cloud_height, receiver_position.y);
    float distance_to_layer = (start_height - receiver_position.y) / max(sunlight.y, 0.05);
    vec3 start = receiver_position + sunlight * distance_to_layer;
    float step_length = (cloud_height + cloud_thickness - start_height) / (4.0 * max(sunlight.y, 0.05));
    float optical_depth = 0.0;
    for (int i = 0; i < 4; i++) {
        float angle = float(i) * 1.5707963;
        vec2 penumbra = vec2(cos(angle), sin(angle)) * cloud_shadow_softness;
        vec3 sample_position = start + sunlight * (float(i) + 0.5) * step_length;
        sample_position.xz += penumbra;
        optical_depth += cloud_density(sample_position, 1.0) * step_length;
    }
    return 1.0 - exp(-optical_depth * 0.9);
}
