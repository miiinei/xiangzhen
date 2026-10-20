extends SceneTree
## Run explicitly after editing StudyTerrain.gd; nothing is baked during play.

func _initialize() -> void:
    var terrain := preload("res://shaders/grassland_study/StudyTerrain.gd").new()
    var started := Time.get_ticks_msec()
    var fields := terrain.bake_fields()
    var result := fields[0].save_png("res://shaders/grassland_study/vegetation.png")
    result |= fields[1].save_png("res://shaders/grassland_study/surface_normals.png")
    # Source composition supplied to imagegen; no light or cloud data is baked in.
    var layout := Image.create(768, 768, false, Image.FORMAT_RGB8)
    for z in range(768):
        for x in range(768):
            var p := (Vector2(x + 0.5, z + 0.5) / 768.0 - Vector2(0.5, 0.5)) * terrain.SPAN
            var cover := terrain.vegetation_at(p).r
            var green := Color(0.46, 0.57, 0.26).lerp(Color(0.21, 0.40, 0.27),
                smoothstep(0.25, 0.85, cover))
            layout.set_pixel(x, z, green)
    result |= layout.save_png("res://shaders/grassland_study/colour_layout.png")
    print("Grassland fields baked in ", Time.get_ticks_msec() - started, " ms; result=", result)
    quit(result)
