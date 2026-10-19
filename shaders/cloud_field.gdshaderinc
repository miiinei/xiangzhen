uniform sampler2D cloud_noise : repeat_enable, filter_linear;
uniform float cloud_scale : hint_range(0.005, 0.2) = 0.035;
uniform vec2 cloud_velocity = vec2(0.22, 0.09);
uniform float cloud_warp : hint_range(0.0, 1.0) = 0.22;
uniform float cloud_coverage : hint_range(0.0, 1.0) = 0.46;
uniform float cloud_softness : hint_range(0.01, 0.4) = 0.18;
uniform float cloud_height = 7.0;
uniform vec3 sun_direction = vec3(-0.287, 0.819, 0.497);

float cloud_density(vec2 world_position) {
    vec2 coordinates = (world_position - TIME * cloud_velocity) * cloud_scale;
    vec2 distortion = texture(cloud_noise, coordinates * 0.43 + vec2(0.17, 0.61)).rg - vec2(0.5);
    float density = texture(cloud_noise, coordinates + distortion * cloud_warp).r;
    return smoothstep(1.0 - cloud_coverage - cloud_softness,
        1.0 - cloud_coverage + cloud_softness, density);
}