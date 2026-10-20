shader_type spatial;
render_mode unshaded, blend_mix, depth_draw_never, cull_disabled, shadows_disabled;

#include "res://shaders/cloud_field.gdshaderinc"

uniform vec4 cloud_color : source_color = vec4(1.0, 0.99, 0.96, 1.0);
uniform float cloud_opacity : hint_range(0.0, 1.0) = 0.8;
uniform float edge_fade : hint_range(0.01, 0.5) = 0.15;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 edge_distance = min(UV, vec2(1.0) - UV);
    float fade = smoothstep(0.0, edge_fade, min(edge_distance.x, edge_distance.y));
    ALBEDO = cloud_color.rgb;
    ALPHA = cloud_density(world_position.xz) * cloud_opacity * fade;
}