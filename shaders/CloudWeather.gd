extends Node

## Zero chooses a new layout each run; any other seed reproduces that layout.
@export var weather_seed: int = 0

var actual_seed: int
var weather_origin: Vector3
var _templates: Array[ShaderMaterial] = []
var _weather_viewport: SubViewport
var _weather_material: ShaderMaterial
var _sunlight_viewport: SubViewport
var _sunlight_material: ShaderMaterial

func _enter_tree() -> void:
    var random := RandomNumberGenerator.new()
    if weather_seed == 0:
        random.randomize()
    else:
        random.seed = weather_seed
    actual_seed = random.seed
    weather_origin = Vector3(random.randf_range(0.0, 64.0),
        random.randf_range(0.0, 64.0), random.randf_range(0.0, 64.0))
    # Set the templates before the parent World creates its tile/unit copies.
    # Retain them until the scene exits: ResourceLoader's cache is weak, and
    # World creates its C# material copies after this child's _ready callback.
    for path in ["res://shaders/cloud_layer.tres", "res://shaders/low_poly.tres"]:
        var material := load(path) as ShaderMaterial
        material.set_shader_parameter("cloud_weather_origin", weather_origin)
        _templates.append(material)
    _create_weather_map()
    _create_sunlight_map()

func _create_weather_map() -> void:
    _weather_material = ShaderMaterial.new()
    _weather_material.shader = load("res://shaders/cloud_weather.gdshader")
    for parameter in _weather_material.shader.get_shader_uniform_list():
        var key: StringName = parameter["name"]
        if key in [&"cloud_weather_map", &"cloud_use_weather_map",
                &"cloud_sunlight_map", &"cloud_use_sunlight_map"]:
            continue
        var value = _templates[0].get_shader_parameter(key)
        if value != null:
            _weather_material.set_shader_parameter(key, value)
    _weather_viewport = SubViewport.new()
    _weather_viewport.name = "CloudCoverage"
    _weather_viewport.size = Vector2i(512, 512)
    _weather_viewport.disable_3d = true
    _weather_viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
    add_child(_weather_viewport)
    var surface := ColorRect.new()
    surface.size = Vector2(512.0, 512.0)
    surface.material = _weather_material
    _weather_viewport.add_child(surface)
    for material in _templates:
        _bind_weather_map(material)

func _bind_weather_map(material: ShaderMaterial) -> void:
    material.set_shader_parameter("cloud_weather_map", _weather_viewport.get_texture())
    material.set_shader_parameter("cloud_use_weather_map", true)

func _create_sunlight_map() -> void:
    _sunlight_material = ShaderMaterial.new()
    _sunlight_material.shader = load("res://shaders/cloud_sunlight.gdshader")
    for parameter in _sunlight_material.shader.get_shader_uniform_list():
        var key: StringName = parameter["name"]
        if key in [&"cloud_sunlight_map", &"cloud_use_sunlight_map"]:
            continue
        var value = _templates[0].get_shader_parameter(key)
        if value != null:
            _sunlight_material.set_shader_parameter(key, value)
    _sunlight_viewport = SubViewport.new()
    _sunlight_viewport.name = "CloudSunlight"
    _sunlight_viewport.size = Vector2i(256, 256)
    _sunlight_viewport.disable_3d = true
    _sunlight_viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
    add_child(_sunlight_viewport)
    var surface := ColorRect.new()
    surface.size = Vector2(256.0, 256.0)
    surface.material = _sunlight_material
    _sunlight_viewport.add_child(surface)
    for material in _templates:
        _bind_sunlight_map(material)

func _bind_sunlight_map(material: ShaderMaterial) -> void:
    material.set_shader_parameter("cloud_sunlight_map", _sunlight_viewport.get_texture())
    material.set_shader_parameter("cloud_use_sunlight_map", true)

func _ready() -> void:
    # The standalone art preview has inline materials instead of factory copies.
    for mesh in get_parent().find_children("*", "MeshInstance3D", true, false):
        var material := mesh.material_override as ShaderMaterial
        if material != null and material.shader.resource_path in [
                "res://shaders/cloud_layer.gdshader", "res://shaders/low_poly.gdshader"]:
            material.set_shader_parameter("cloud_weather_origin", weather_origin)
            _bind_weather_map(material)
            _bind_sunlight_map(material)
