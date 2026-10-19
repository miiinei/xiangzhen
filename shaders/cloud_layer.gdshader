shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, depth_test_disabled, cull_disabled, fog_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

// Ray integration and sunlight extinction adapted for a local, downward-looking
// cloud layer from the MIT references in CLOUDS_LICENSES.md.
uniform sampler2D scene_depth : hint_depth_texture, repeat_disable, filter_nearest;
uniform vec4 cloud_color : source_color = vec4(0.965, 0.975, 0.985, 1.0);
uniform vec4 cloud_shade_color : source_color = vec4(0.28, 0.34, 0.42, 1.0);
uniform float cloud_opacity : hint_range(0.0, 1.0) = 0.65;
uniform float unit_cloud_opacity : hint_range(0.0, 1.0) = 0.25;
uniform float unit_far_cloud_opacity : hint_range(0.0, 1.0) = 1.0;
uniform float cloud_extinction : hint_range(0.1, 4.0) = 1.2;
uniform float cloud_light_extinction : hint_range(0.1, 6.0) = 1.6;
uniform int cloud_light_steps : hint_range(2, 8, 1) = 4;
uniform int cloud_steps : hint_range(8, 48, 4) = 24;
uniform float cloud_far_opacity : hint_range(0.0, 1.0) = 1.0;
uniform vec2 cloud_zoom_range = vec2(20.0, 40.0);
uniform float cloud_near_thickness_scale : hint_range(0.05, 1.0) = 0.34375;
uniform float cloud_far_detail : hint_range(0.0, 1.0) = 0.65;

// Visual thinning for close-up play. The reference cloud field and the ground
// shadows retain the full thickness even when this layer becomes transparent.
float view_cloud_density(vec3 world_position, float thickness_scale, float detail_contrast) {
    world_position.y = cloud_height + (world_position.y - cloud_height) / thickness_scale;
    return cloud_density(world_position, detail_contrast) * thickness_scale;
}

void vertex() {
    // A camera-child quad supplies coverage; density remains in world space.
    POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
    // Orthographic vertical extent follows the existing Camera3D zoom. No
    // per-frame C# update or material synchronization is needed.
    float view_size = 2.0 / max(abs(PROJECTION_MATRIX[1][1]), 0.0001);
    float zoom_blend = smoothstep(cloud_zoom_range.x,
        max(cloud_zoom_range.y, cloud_zoom_range.x + 0.01), view_size);
    float thickness_scale = mix(cloud_near_thickness_scale, 1.0, zoom_blend);
    float view_thickness = cloud_thickness * thickness_scale;
    float view_opacity = mix(cloud_opacity, cloud_far_opacity, zoom_blend);
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

    float base_distance = (cloud_height - ray_origin.y) / ray_direction.y;
    float top_distance = (cloud_height + view_thickness - ray_origin.y) / ray_direction.y;
    float entry = max(min(base_distance, top_distance), 0.0);
    float exit_distance = max(base_distance, top_distance);

    float depth = texture(scene_depth, SCREEN_UV).r;
    vec4 surface_view = INV_PROJECTION_MATRIX * vec4(screen, depth * 2.0 - 1.0, 1.0);
    vec3 surface_world = (INV_VIEW_MATRIX * vec4(surface_view.xyz / surface_view.w, 1.0)).xyz;
    exit_distance = min(exit_distance, dot(surface_world - ray_origin, ray_direction));
    if (exit_distance <= entry) {
        discard;
    }

    float step_length = (exit_distance - entry) / float(cloud_steps);
    vec3 sunlight = normalize(sun_direction);
    // Fixed screen-space jitter breaks up slice banding without time flicker.
    float ray_offset = fract(sin(dot(FRAGCOORD.xy, vec2(12.9898, 78.233))) * 43758.5453);
    vec3 accumulated_color = vec3(0.0);
    float transmission = 1.0;
    for (int i = 0; i < cloud_steps; i++) {
        vec3 sample_position = ray_origin + ray_direction * (entry + (float(i) + ray_offset) * step_length);
        float density = view_cloud_density(sample_position, thickness_scale, detail_contrast);
        if (density < 0.001) {
            continue;
        }
        float sunlight_depth = 0.0;
        float light_distance = (cloud_height + view_thickness - sample_position.y)
            / max(sunlight.y, 0.2);
        // Concentrate sun samples near the visible point to resolve billow
        // self-shadowing. View opacity and sun absorption are independent.
        for (int j = 0; j < cloud_light_steps; j++) {
            float a = float(j) / float(cloud_light_steps);
            float b = float(j + 1) / float(cloud_light_steps);
            float segment_length = (b * b - a * a) * light_distance;
            vec3 light_position = sample_position + sunlight * (a * a + b * b) * 0.5 * light_distance;
            sunlight_depth += view_cloud_density(light_position, thickness_scale, detail_contrast)
                * segment_length;
        }
        float light_depth = sunlight_depth * cloud_light_extinction;
        float direct_light = exp(-light_depth);
        float scattered_fill = 0.12 * exp(-light_depth * 0.35);
        float altitude = (sample_position.y - cloud_height) / view_thickness;
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
    float alpha = 1.0 - transmission;
    if (alpha < 0.001) {
        discard;
    }
    ALBEDO = accumulated_color / alpha;
    // Current board tops are at y=0.08. Protect raised unit silhouettes nearby;
    // the protection fades at distant zoom so thick clouds can obscure units.
    float raised_surface = smoothstep(0.18, 0.5, surface_world.y);
    float unit_opacity = mix(unit_cloud_opacity, unit_far_cloud_opacity, zoom_blend);
    ALPHA = alpha * view_opacity * mix(1.0, unit_opacity, raised_surface);
}
