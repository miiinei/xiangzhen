shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, depth_test_disabled, cull_disabled, fog_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

// Ray integration and sunlight extinction adapted for a local, downward-looking
// cloud layer from the MIT references in CLOUDS_LICENSES.md.
uniform sampler2D scene_depth : hint_depth_texture, repeat_disable, filter_nearest;
uniform vec4 cloud_color : source_color = vec4(0.965, 0.975, 0.985, 1.0);
uniform vec4 cloud_shade_color : source_color = vec4(0.28, 0.34, 0.42, 1.0);
uniform float cloud_opacity : hint_range(0.0, 1.0) = 0.38;
uniform float unit_cloud_opacity : hint_range(0.0, 1.0) = 0.08;
uniform float unit_far_cloud_opacity : hint_range(0.0, 1.0) = 1.0;
uniform float cloud_extinction : hint_range(0.1, 4.0) = 1.2;
uniform float cloud_light_extinction : hint_range(0.1, 6.0) = 1.6;
uniform int cloud_light_steps : hint_range(2, 8, 1) = 4;
uniform int cloud_steps : hint_range(8, 48, 4) = 24;
uniform int cloud_upper_steps : hint_range(8, 32, 4) = 16;
uniform float cloud_upper_opacity : hint_range(0.0, 1.0) = 1.0;
uniform float cloud_far_opacity : hint_range(0.0, 1.0) = 1.0;
uniform vec2 cloud_zoom_range = vec2(20.0, 40.0);
uniform vec2 cloud_close_fade_range = vec2(8.0, 14.0);
uniform float cloud_near_thickness_scale : hint_range(0.05, 1.0) = 0.34375;
uniform float cloud_far_detail : hint_range(0.0, 1.0) = 0.65;

// Cirrus is an artistic distant backdrop. It is absent over scene geometry and
// never participates in the two physical bands' sunlight or ground shadows.
uniform vec2 cloud_cirrus_zoom_range = vec2(34.0, 40.0);
uniform float cloud_cirrus_opacity : hint_range(0.0, 0.5) = 0.16;
uniform vec4 cloud_cirrus_color : source_color = vec4(0.88, 0.94, 0.985, 1.0);
uniform float cloud_cirrus_height = 9.0;

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

float distant_cirrus(vec2 world_position) {
    vec2 wind = cloud_rotated_wind(0.45);
    vec2 across_wind = vec2(-wind.y, wind.x);
    vec2 coordinates = (world_position - TIME * wind * cloud_speed * 1.8)
        * cloud_scale + cloud_weather_origin.xz + vec2(0.81, 0.27);
    float evolution = TIME * cloud_evolution_speed;
    vec2 warp = vec2(texture(cloud_noise, coordinates * 0.6 + vec2(0.13, 0.51)).r,
        texture(cloud_noise, coordinates * 0.6 + vec2(0.79, 0.31)).r) - vec2(0.5);
    vec2 stretched = vec2(dot(coordinates, wind) * 0.22,
        dot(coordinates, across_wind) * 2.8) + warp * 0.6;
    float patch = smoothstep(0.38, 0.68,
        texture(cloud_noise, coordinates * 0.35 + vec2(0.53, 0.17)).r);
    float strands = texture(cloud_noise, stretched
        + evolution * vec2(0.015, -0.07)).r * 0.75
        + texture(cloud_noise, stretched * 2.3 + vec2(0.31, 0.73)
            + evolution * vec2(-0.02, 0.08)).r * 0.25;
    return patch * smoothstep(0.36, 0.72, strands);
}

void vertex() {
    POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
    float view_size = 2.0 / max(abs(PROJECTION_MATRIX[1][1]), 0.0001);
    float zoom_blend = smoothstep(cloud_zoom_range.x,
        max(cloud_zoom_range.y, cloud_zoom_range.x + 0.01), view_size);
    float thickness_scale = mix(cloud_near_thickness_scale, 1.0, zoom_blend);
    // The camera now stays above the low clouds even at its closest zoom.
    // Fade their display explicitly; physical density and shadows stay intact.
    float close_fade = smoothstep(cloud_close_fade_range.x,
        max(cloud_close_fade_range.y, cloud_close_fade_range.x + 0.01), view_size);
    float view_opacity = mix(cloud_opacity, cloud_far_opacity, zoom_blend) * close_fade;
    if (view_opacity < 0.001) {
        discard;
    }
    float detail_contrast = mix(1.0, cloud_far_detail, zoom_blend);
    vec2 screen = SCREEN_UV * 2.0 - 1.0;
    // Godot 4.3+ uses reversed Z. Compatibility NDC spans [-1, 1].
    vec4 near_view = INV_PROJECTION_MATRIX * vec4(screen, 1.0, 1.0);
    vec4 far_view = INV_PROJECTION_MATRIX * vec4(screen, -1.0, 1.0);
    vec3 ray_origin = (INV_VIEW_MATRIX * vec4(near_view.xyz / near_view.w, 1.0)).xyz;
    vec3 far_world = (INV_VIEW_MATRIX * vec4(far_view.xyz / far_view.w, 1.0)).xyz;
    vec3 ray_direction = normalize(far_world - ray_origin);
    if (abs(ray_direction.y) < 0.001) {
        discard;
    }
    // An orthographic near plane is tilted in world space. Its lower edge can
    // cut through the clouds even while the camera is above them, producing a
    // screen-wide cutoff. Move each origin along its unchanged viewing ray to
    // the camera's horizontal height plane, then clip against scene depth.
    float camera_plane_distance = (INV_VIEW_MATRIX[3].y - ray_origin.y) / ray_direction.y;
    ray_origin += ray_direction * camera_plane_distance;
    float depth = texture(scene_depth, SCREEN_UV).r;
    vec4 surface_view = INV_PROJECTION_MATRIX * vec4(screen, depth * 2.0 - 1.0, 1.0);
    vec3 surface_world = (INV_VIEW_MATRIX * vec4(surface_view.xyz / surface_view.w, 1.0)).xyz;
    float surface_distance = dot(surface_world - ray_origin, ray_direction);
    float ray_offset = mix(0.5,
        fract(sin(dot(FRAGCOORD.xy, vec2(12.9898, 78.233))) * 43758.5453), 0.35);

    vec4 upper = vec4(0.0);
    if (cloud_upper_opacity > 0.0) {
        upper = march_cloud_band(ray_origin, ray_direction, surface_distance,
            1.0, thickness_scale, detail_contrast, ray_offset) * cloud_upper_opacity;
    }
    vec4 lower = vec4(0.0);
    if (upper.a < 0.99) {
        lower = march_cloud_band(ray_origin, ray_direction, surface_distance,
            0.0, thickness_scale, detail_contrast, ray_offset);
    }
    vec3 accumulated_color = upper.rgb + (1.0 - upper.a) * lower.rgb;
    float alpha = upper.a + (1.0 - upper.a) * lower.a;

    float cirrus_blend = smoothstep(cloud_cirrus_zoom_range.x,
        max(cloud_cirrus_zoom_range.y, cloud_cirrus_zoom_range.x + 0.01), view_size);
    if (cirrus_blend > 0.0 && depth < 0.0001 && cloud_cirrus_opacity > 0.0) {
        float cirrus_distance = (cloud_cirrus_height - ray_origin.y) / ray_direction.y;
        if (cirrus_distance > 0.0) {
            vec2 cirrus_position = (ray_origin + ray_direction * cirrus_distance).xz;
            float cirrus_alpha = distant_cirrus(cirrus_position)
                * cloud_cirrus_opacity * cirrus_blend;
            accumulated_color += (1.0 - alpha) * cloud_cirrus_color.rgb * cirrus_alpha;
            alpha += (1.0 - alpha) * cirrus_alpha;
        }
    }
    if (alpha < 0.001) {
        discard;
    }
    ALBEDO = accumulated_color / alpha;
    // Near tactical play protects raised unit silhouettes. Far overview permits
    // physical clouds to obscure units, while cirrus stays behind the board.
    float raised_surface = smoothstep(0.18, 0.5, surface_world.y);
    float unit_opacity = mix(unit_cloud_opacity, unit_far_cloud_opacity, zoom_blend);
    ALPHA = alpha * view_opacity * mix(1.0, unit_opacity, raised_surface);
}
