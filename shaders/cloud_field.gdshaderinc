// Density shaping and light integration reference the MIT cloud demos listed in
// CLOUDS_LICENSES.md. Noise textures are generated locally by Godot.
uniform sampler2D cloud_noise : repeat_enable, filter_linear;
uniform sampler3D cloud_volume_noise : repeat_enable, filter_linear;
uniform sampler3D cloud_base_noise : repeat_enable, filter_linear;
uniform vec3 cloud_weather_origin = vec3(0.0);
uniform sampler2D cloud_sunlight_map : repeat_disable, filter_linear;
uniform bool cloud_use_sunlight_map = false;
uniform vec2 cloud_sunlight_origin = vec2(-48.0);
uniform vec2 cloud_sunlight_span = vec2(128.0);
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
uniform float cloud_upper_height = 11.3;
uniform float cloud_upper_thickness : hint_range(0.5, 8.0) = 2.4;
uniform float cloud_upper_coverage : hint_range(0.0, 1.0) = 0.42;
uniform float cloud_upper_scale : hint_range(0.5, 2.0) = 1.15;
uniform float cloud_upper_density : hint_range(0.0, 2.0) = 0.95;
uniform float cloud_upper_speed_multiplier : hint_range(0.0, 3.0) = 1.35;
uniform float cloud_upper_wind_turn : hint_range(-1.57, 1.57) = 0.22;
uniform float cloud_vertical_shear : hint_range(0.0, 0.3) = 0.08;
uniform float cloud_upper_shadow_strength : hint_range(0.0, 1.0) = 1.0;
uniform float cloud_shadow_softness : hint_range(0.0, 3.0) = 0.65;
uniform vec3 sun_direction = vec3(-0.287, 0.819, 0.497);

vec2 cloud_wind_direction() {
    return grass_wind_direction / max(length(grass_wind_direction), 0.001);
}

vec2 cloud_rotated_wind(float angle) {
    vec2 wind = cloud_wind_direction();
    return vec2(wind.x * cos(angle) - wind.y * sin(angle),
        wind.x * sin(angle) + wind.y * cos(angle));
}

float cloud_band_height(float band) {
    return mix(cloud_height, cloud_upper_height, band);
}

float cloud_band_thickness(float band) {
    return mix(cloud_thickness, cloud_upper_thickness, band);
}

vec2 cloud_band_coordinates(vec2 world_position, float band) {
    vec2 wind = cloud_rotated_wind(cloud_upper_wind_turn * band);
    float speed = cloud_speed * mix(1.0, cloud_upper_speed_multiplier, band);
    float scale = cloud_scale * mix(1.0, cloud_upper_scale, band);
    return (world_position - TIME * wind * speed) * scale
        + cloud_weather_origin.xz + band * vec2(0.37, 0.61);
}

// Continuous weather has no placement cells or individual cloud outlines.
// Broad gaps, broken patches and stretched filaments share one warped field.
vec2 cloud_weather(vec2 coordinates, vec2 world_position, float band) {
    float evolution = TIME * cloud_evolution_speed * mix(1.0, 0.85, band);
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
    if (band > 0.5) {
        // A shared slow humidity pattern gives the layers related clear/cloudy
        // regions, while their local weather and billows remain independent.
        vec2 humidity = (world_position - TIME * cloud_wind_direction()
            * cloud_speed * 0.55) * cloud_scale + cloud_weather_origin.xz;
        float shared_weather = texture(cloud_noise,
            humidity * 0.3 + vec2(0.31, 0.19)).r;
        broad = mix(broad, shared_weather, 0.55);
    }
    float broken = texture(cloud_noise, p * 1.1 + vec2(0.73, 0.47)
        + evolution * vec2(-0.06, 0.035)).r;
    float field = broad * 0.72 + broken * 0.28;
    float threshold = mix(0.69, 0.35, mix(cloud_coverage, cloud_upper_coverage, band));
    float envelope = smoothstep(threshold - cloud_softness * 0.25,
        threshold + cloud_softness * 0.6, field);
    float veil = smoothstep(threshold - 0.14, threshold + cloud_softness * 0.25, field);

    vec2 wind = cloud_rotated_wind(cloud_upper_wind_turn * band);
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
float cloud_band_density(vec3 world_position, float band, float detail_contrast) {
    float base_height = cloud_band_height(band);
    float thickness = cloud_band_thickness(band);
    float altitude = (world_position.y - base_height) / thickness;
    if (altitude <= 0.0 || altitude >= 1.0) {
        return 0.0;
    }
    float density_scale = mix(1.0, cloud_upper_density, band);
    if (density_scale <= 0.0) {
        return 0.0;
    }
    vec2 coordinates = cloud_band_coordinates(world_position.xz, band);
    vec2 weather = cloud_weather(coordinates, world_position.xz, band);
    if (max(weather.x, weather.y) < 0.005) {
        return 0.0;
    }
    // Weather only controls horizontal coverage. The cloud's bulk is sampled
    // in isotropic 3D coordinates, rather than extruding a 2D height field.
    float profile = pow(sin(altitude * 3.14159265), 0.75);
    float wisp_profile = exp(-pow((altitude - 0.62) * 9.0, 2.0));
    float scale = cloud_scale * mix(1.0, cloud_upper_scale, band);
    vec2 wind = cloud_rotated_wind(cloud_upper_wind_turn * band);
    float speed = cloud_speed * mix(1.0, cloud_upper_speed_multiplier, band);
    vec2 sheared = coordinates - TIME * wind * speed * scale
        * (altitude - 0.5) * cloud_vertical_shear;
    vec3 p = vec3(sheared.x, (world_position.y - base_height) * scale,
        sheared.y) * mix(2.8, 3.2, band)
        + vec3(0.0, cloud_weather_origin.y, 0.0) + band * vec3(0.43, 0.27, 0.79);
    float evolution = TIME * cloud_evolution_speed * mix(1.0, 0.85, band);
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
    float base_shape = mix(coherent, billows, mix(0.45, 0.65, band)) * profile;
    float threshold = mix(0.78, mix(0.24, 0.30, band), weather.x);
    float edge_erosion = (1.0 - base_shape) * (1.0 - base_shape)
        * (1.0 - erosion) * 0.12;
    float body_shape = (base_shape - threshold - edge_erosion) / (1.0 - threshold);
    float body_presence = smoothstep(0.0, 0.08, weather.x)
        * smoothstep(0.0, 0.08, profile);
    float body = cloud_soft_density(body_shape) * body_presence * 2.8;
    float wisps = weather.y * wisp_profile * mix(0.65, 1.0, erosion) * 0.18;
    return (body + wisps) * density_scale;
}

// Light always traverses the full reference bands, independently of the camera's
// visual compression. Nonuniform sampling resolves nearby self-shadowing.
float cloud_band_sun_depth(vec3 receiver_position, float band, int steps) {
    vec3 sunlight = normalize(sun_direction);
    float base_height = cloud_band_height(band);
    float top_height = base_height + cloud_band_thickness(band);
    if (receiver_position.y >= top_height) {
        return 0.0;
    }
    float start_distance = max((base_height - receiver_position.y) / max(sunlight.y, 0.05), 0.0);
    float distance_in_band = (top_height - max(base_height, receiver_position.y))
        / max(sunlight.y, 0.05);
    float optical_depth = 0.0;
    for (int i = 0; i < steps; i++) {
        float a = float(i) / float(steps);
        float b = float(i + 1) / float(steps);
        float segment_length = (b * b - a * a) * distance_in_band;
        vec3 p = receiver_position + sunlight
            * (start_distance + (a * a + b * b) * 0.5 * distance_in_band);
        optical_depth += cloud_band_density(p, band, 1.0) * segment_length;
    }
    return optical_depth;
}

float cloud_band_shadow_depth(vec3 receiver_position, float band) {
    vec3 sunlight = normalize(sun_direction);
    float base_height = cloud_band_height(band);
    float top_height = base_height + cloud_band_thickness(band);
    if (receiver_position.y >= top_height) {
        return 0.0;
    }
    float start_height = max(base_height, receiver_position.y);
    float distance_to_layer = (start_height - receiver_position.y) / max(sunlight.y, 0.05);
    vec3 start = receiver_position + sunlight * distance_to_layer;
    float step_length = (top_height - start_height) / (4.0 * max(sunlight.y, 0.05));
    float optical_depth = 0.0;
    for (int i = 0; i < 4; i++) {
        float angle = float(i) * 1.5707963;
        vec2 penumbra = vec2(cos(angle), sin(angle)) * cloud_shadow_softness;
        vec3 sample_position = start + sunlight * (float(i) + 0.5) * step_length;
        sample_position.xz += penumbra;
        optical_depth += cloud_band_density(sample_position, band, 1.0) * step_length;
    }
    return optical_depth;
}

vec2 cloud_sunlight_uv(vec3 receiver_position) {
    vec3 sunlight = normalize(sun_direction);
    vec2 ground_ray = receiver_position.xz
        - sunlight.xz * receiver_position.y / max(sunlight.y, 0.05);
    return (ground_ray - cloud_sunlight_origin) / cloud_sunlight_span;
}

bool cloud_has_sunlight_cache(vec2 uv) {
    return cloud_use_sunlight_map && min(uv.x, uv.y) > 0.002
        && max(uv.x, uv.y) < 0.998;
}

float cloud_upper_sun_depth(vec3 receiver_position) {
    vec2 uv = cloud_sunlight_uv(receiver_position);
    if (receiver_position.y <= cloud_upper_height && cloud_has_sunlight_cache(uv)) {
        return -log(max(texture(cloud_sunlight_map, uv).b, 0.001));
    }
    return cloud_band_sun_depth(receiver_position, 1.0, 3);
}

float cloud_shadow(vec3 receiver_position) {
    vec2 uv = cloud_sunlight_uv(receiver_position);
    if (receiver_position.y <= min(cloud_height, cloud_upper_height)
            && cloud_has_sunlight_cache(uv)) {
        vec2 transmission = max(texture(cloud_sunlight_map, uv).rg, vec2(0.001));
        return 1.0 - pow(transmission.r
            * pow(transmission.g, cloud_upper_shadow_strength), 0.9);
    }
    float optical_depth = cloud_band_shadow_depth(receiver_position, 0.0)
        + cloud_band_shadow_depth(receiver_position, 1.0) * cloud_upper_shadow_strength;
    return 1.0 - exp(-optical_depth * 0.9);
}
