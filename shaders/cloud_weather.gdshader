shader_type canvas_item;
render_mode unshaded, blend_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

// RG/BA store the lower/upper bands' coverage and filament envelopes. These
// slow horizontal fields are shared by all view, sunlight and shadow samples.
void fragment() {
    vec2 world_position = cloud_sunlight_origin + UV * cloud_sunlight_span;
    vec2 lower = cloud_weather(cloud_band_coordinates(world_position, 0.0),
        world_position, 0.0);
    vec2 upper = cloud_weather(cloud_band_coordinates(world_position, 1.0),
        world_position, 1.0);
    COLOR = vec4(lower, upper);
}
