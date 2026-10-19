extends Node

## Zero chooses a new layout each run; any other seed reproduces that layout.
@export var weather_seed: int = 0

var actual_seed: int
var weather_origin: Vector3
var _templates: Array[ShaderMaterial] = []

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

func _ready() -> void:
    # The standalone art preview has inline materials instead of factory copies.
    for mesh in get_parent().find_children("*", "MeshInstance3D", true, false):
        var material := mesh.material_override as ShaderMaterial
        if material != null and material.shader.resource_path in [
                "res://shaders/cloud_layer.gdshader", "res://shaders/low_poly.gdshader"]:
            material.set_shader_parameter("cloud_weather_origin", weather_origin)
