shader_type canvas_item;
render_mode unshaded, blend_disabled;

#include "res://shaders/cloud_optics.gdshaderinc"

uniform vec3 cloud_view_corner;
uniform vec3 cloud_view_u;
uniform vec3 cloud_view_v;
uniform vec3 cloud_view_direction;
uniform float cloud_view_camera_height;
uniform float cloud_view_size;

// Store lower/upper premultiplied color and alpha in separate atlas halves.
// Depth and unit protection are applied by the live spatial compositor.
void fragment() {
    float band = step(0.5, UV.y);
    vec2 uv = vec2(UV.x, fract(UV.y * 2.0));
    vec3 ray_origin = cloud_view_corner + uv.x * cloud_view_u + uv.y * cloud_view_v;
    ray_origin += cloud_view_direction
        * ((cloud_view_camera_height - ray_origin.y) / cloud_view_direction.y);
    float zoom_blend = smoothstep(cloud_zoom_range.x,
        max(cloud_zoom_range.y, cloud_zoom_range.x + 0.01), cloud_view_size);
    float thickness_scale = mix(cloud_near_thickness_scale, 1.0, zoom_blend);
    float detail_contrast = mix(1.0, cloud_far_detail, zoom_blend);
    float ray_offset = mix(0.5,
        fract(sin(dot(FRAGCOORD.xy, vec2(12.9898, 78.233))) * 43758.5453), 0.35);
    COLOR = march_cloud_band(ray_origin, cloud_view_direction, 10000.0,
        band, thickness_scale, detail_contrast, ray_offset);
}
