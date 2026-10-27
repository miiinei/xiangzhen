#include "res://shaders/cloud_field.gdshaderinc"

// Ray integration and sunlight extinction adapted for a local, downward-looking
// cloud layer from the MIT references in CLOUDS_LICENSES.md.
uniform vec4 cloud_color : source_color = vec4(0.965, 0.975, 0.985, 1.0);
uniform vec4 cloud_shade_color : source_color = vec4(0.28, 0.34, 0.42, 1.0);
uniform float cloud_extinction : hint_range(0.1, 4.0) = 1.2;
uniform float cloud_light_extinction : hint_range(0.1, 6.0) = 1.6;
uniform int cloud_light_steps : hint_range(2, 8, 1) = 4;
uniform int cloud_steps : hint_range(8, 48, 4) = 24;
uniform int cloud_upper_steps : hint_range(8, 32, 4) = 16;
uniform vec2 cloud_zoom_range = vec2(20.0, 40.0);
// CloudWeather also reads this default to skip the closest-zoom view cache.
uniform vec2 cloud_close_fade_range = vec2(8.0, 14.0);
uniform float cloud_near_thickness_scale : hint_range(0.05, 1.0) = 0.34375;
uniform float cloud_far_detail : hint_range(0.0, 1.0) = 0.65;

vec3 cloud_reference_position(vec3 world_position, float band, float thickness_scale) {
    float base_height = cloud_band_height(band);
    world_position.y = base_height + (world_position.y - base_height) / thickness_scale;
    return world_position;
}

float view_cloud_density(vec3 world_position, float band, float thickness_scale, float detail_contrast) {
    return cloud_band_density(cloud_reference_position(world_position, band, thickness_scale),
        band, detail_contrast) * thickness_scale;
}

// Each occupied band gets its own sample budget: no samples are spent in the
// clear gap. The upper band is composited before the lower one, along the ray.
vec4 march_cloud_band(vec3 ray_origin, vec3 ray_direction, float surface_distance,
        float band, float thickness_scale, float detail_contrast, float ray_offset) {
    if (band > 0.5 && cloud_upper_density <= 0.0) {
        return vec4(0.0);
    }
    float base_height = cloud_band_height(band);
    float view_thickness = cloud_band_thickness(band) * thickness_scale;
    float base_distance = (base_height - ray_origin.y) / ray_direction.y;
    float top_distance = (base_height + view_thickness - ray_origin.y) / ray_direction.y;
    float entry = max(min(base_distance, top_distance), 0.0);
    float exit_distance = min(max(base_distance, top_distance), surface_distance);
    if (exit_distance <= entry) {
        return vec4(0.0);
    }

    int steps = band > 0.5 ? cloud_upper_steps : cloud_steps;
    int light_steps = band > 0.5 ? max(cloud_light_steps - 1, 2) : cloud_light_steps;
    float step_length = (exit_distance - entry) / float(steps);
    vec3 accumulated_color = vec3(0.0);
    float transmission = 1.0;
    for (int i = 0; i < steps; i++) {
        vec3 sample_position = ray_origin + ray_direction
            * (entry + (float(i) + ray_offset) * step_length);
        float density = view_cloud_density(sample_position, band, thickness_scale, detail_contrast);
        if (density < 0.001) {
            continue;
        }
        // Visible compression changes neither the physical point's lighting
        // nor the upper cloud's ability to shadow this lower cloud.
        vec3 reference_position = cloud_reference_position(sample_position, band, thickness_scale);
        float sunlight_depth = cloud_band_sun_depth(reference_position, band, light_steps);
        if (band < 0.5) {
            sunlight_depth += cloud_upper_sun_depth(reference_position)
                * cloud_upper_shadow_strength;
        }
        float light_depth = sunlight_depth * cloud_light_extinction;
        float direct_light = exp(-light_depth);
        float scattered_fill = 0.12 * exp(-light_depth * 0.35);
        float altitude = (sample_position.y - base_height) / view_thickness;
        float illumination = clamp(0.14 + altitude * 0.12 + direct_light * 0.72
            + scattered_fill, 0.0, 1.0);
        vec3 sample_color = mix(cloud_shade_color.rgb, cloud_color.rgb, illumination);
        float sample_alpha = 1.0 - exp(-density * step_length * cloud_extinction);
        accumulated_color += transmission * sample_alpha * sample_color;
        transmission *= 1.0 - sample_alpha;
        if (transmission < 0.01) {
            break;
        }
    }
    return vec4(accumulated_color, 1.0 - transmission);
}
