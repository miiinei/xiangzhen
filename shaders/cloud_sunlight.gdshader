shader_type canvas_item;
render_mode unshaded;

#include "res://shaders/cloud_field.gdshaderinc"

// R/G store the full lower/upper bands' transmission for receiver shadows.
// B stores unblurred upper transmission used to illuminate the lower cloud.
// All rays are indexed by their intersection with the world y=0 plane.
void fragment() {
    vec2 world_position = cloud_sunlight_origin + UV * cloud_sunlight_span;
    vec3 receiver = vec3(world_position.x, 0.0, world_position.y);
    float lower_depth = cloud_band_shadow_depth(receiver, 0.0);
    float upper_depth = cloud_band_shadow_depth(receiver, 1.0);
    float upper_sun_depth = cloud_band_sun_depth(receiver, 1.0, 3);
    COLOR = vec4(exp(-vec3(lower_depth, upper_depth, upper_sun_depth)), 1.0);
}
