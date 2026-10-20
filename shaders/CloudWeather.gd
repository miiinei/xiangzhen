extends Node

## Zero chooses a new layout each run; any other seed reproduces that layout.
@export var weather_seed: int = 0
## Only cloud work is throttled; the scene and camera retain their own cadence.
@export_range(5.0, 60.0, 1.0) var cloud_update_fps: float = 20.0
@export_range(0.5, 1.0, 0.05) var cloud_resolution_scale: float = 0.75

const VIEW_OVERSCAN: float = 1.2

var actual_seed: int
var weather_origin: Vector3
var _templates: Array[ShaderMaterial] = []
var _weather_viewport: SubViewport
var _weather_material: ShaderMaterial
var _sunlight_viewport: SubViewport
var _sunlight_material: ShaderMaterial
var _view_viewport: SubViewport
var _view_material: ShaderMaterial
var _view_receivers: Array[ShaderMaterial] = []
var _clock_image: Image
var _clock_texture: ImageTexture
var _cloud_time: float = 0.0
var _tick_elapsed: float = 0.0
var _view_camera: Camera3D
var _view_transform: Transform3D
var _view_camera_size: float = -1.0
var _view_screen_size := Vector2i.ZERO
var _view_valid: bool = false

func _enter_tree() -> void:
    # Run after World's camera movement, without changing any global timing.
    process_priority = 100
    _clock_image = Image.create(1, 1, false, Image.FORMAT_RF)
    _clock_image.fill(Color(0.0, 0.0, 0.0, 1.0))
    _clock_texture = ImageTexture.create_from_image(_clock_image)
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
        _bind_clock(material)
        _templates.append(material)
    _view_receivers.append(_templates[0])
    _create_weather_map()
    _create_sunlight_map()
    _create_view_map()

func _bind_clock(material: ShaderMaterial) -> void:
    material.set_shader_parameter("cloud_clock", _clock_texture)
    material.set_shader_parameter("cloud_use_clock", true)

func _create_cache_material(shader_path: String,
        excluded_parameters: Array[StringName] = []) -> ShaderMaterial:
    var material := ShaderMaterial.new()
    material.shader = load(shader_path)
    for parameter in material.shader.get_shader_uniform_list():
        var key: StringName = parameter["name"]
        if key in excluded_parameters:
            continue
        var value = _templates[0].get_shader_parameter(key)
        if value != null:
            material.set_shader_parameter(key, value)
    return material

func _create_cache_viewport(cache_name: String, material: ShaderMaterial,
        cache_size: Vector2i,
        update_mode: SubViewport.UpdateMode = SubViewport.UPDATE_ONCE) -> SubViewport:
    var viewport := SubViewport.new()
    viewport.name = cache_name
    viewport.size = cache_size
    viewport.disable_3d = true
    viewport.render_target_update_mode = update_mode
    add_child(viewport)
    var surface := ColorRect.new()
    surface.material = material
    viewport.add_child(surface)
    # Let the rectangle follow its viewport, including cloud-view resizes.
    surface.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
    return viewport

func _create_weather_map() -> void:
    # Exclude downstream textures so a pass never reads its own render target.
    _weather_material = _create_cache_material("res://shaders/cloud_weather.gdshader",
        [&"cloud_weather_map", &"cloud_use_weather_map",
        &"cloud_sunlight_map", &"cloud_use_sunlight_map"])
    _weather_viewport = _create_cache_viewport("CloudCoverage", _weather_material,
        Vector2i(512, 512))
    for material in _templates:
        _bind_weather_map(material)

func _bind_weather_map(material: ShaderMaterial) -> void:
    material.set_shader_parameter("cloud_weather_map", _weather_viewport.get_texture())
    material.set_shader_parameter("cloud_use_weather_map", true)

func _create_sunlight_map() -> void:
    _sunlight_material = _create_cache_material("res://shaders/cloud_sunlight.gdshader",
        [&"cloud_sunlight_map", &"cloud_use_sunlight_map"])
    _sunlight_viewport = _create_cache_viewport("CloudSunlight", _sunlight_material,
        Vector2i(256, 256))
    for material in _templates:
        _bind_sunlight_map(material)

func _bind_sunlight_map(material: ShaderMaterial) -> void:
    material.set_shader_parameter("cloud_sunlight_map", _sunlight_viewport.get_texture())
    material.set_shader_parameter("cloud_use_sunlight_map", true)

func _create_view_map() -> void:
    _view_material = _create_cache_material("res://shaders/cloud_view.gdshader")
    _view_viewport = _create_cache_viewport("CloudView", _view_material,
        Vector2i(2, 2), SubViewport.UPDATE_DISABLED)
    _view_viewport.transparent_bg = true
    for material in _view_receivers:
        material.set_shader_parameter("cloud_view_map", _view_viewport.get_texture())

func _process(delta: float) -> void:
    _cloud_time += delta
    _tick_elapsed += delta
    var interval := 1.0 / cloud_update_fps
    var tick := _tick_elapsed >= interval
    if tick:
        _tick_elapsed = fmod(_tick_elapsed, interval)
        _clock_image.set_pixel(0, 0, Color(_cloud_time, 0.0, 0.0, 1.0))
        _clock_texture.update(_clock_image)
        _weather_viewport.render_target_update_mode = SubViewport.UPDATE_ONCE
        _sunlight_viewport.render_target_update_mode = SubViewport.UPDATE_ONCE
    _update_view(tick)

func _update_view(tick: bool) -> void:
    var camera := get_viewport().get_camera_3d()
    if camera == null or camera.projection != Camera3D.PROJECTION_ORTHOGONAL:
        _set_view_enabled(false)
        _view_valid = false
        return
    var screen_size := Vector2i(get_viewport().get_visible_rect().size)
    var transform := camera.global_transform
    var changed := not _view_valid or camera != _view_camera \
        or screen_size != _view_screen_size or camera.size != _view_camera_size \
        or not transform.basis.is_equal_approx(_view_transform.basis) \
        or not is_equal_approx(transform.origin.y, _view_transform.origin.y)
    # Reproject small pans at full scene speed. Refresh immediately after a
    # teleport or before a fast pan can expose the overscan margin.
    if _view_valid and not changed:
        var move := transform.origin - _view_transform.origin
        var span_u: Vector3 = _view_material.get_shader_parameter("cloud_view_u")
        var span_v: Vector3 = _view_material.get_shader_parameter("cloud_view_v")
        changed = absf(move.dot(span_u.normalized())) > span_u.length() * 0.07 \
            or absf(move.dot(span_v.normalized())) > span_v.length() * 0.07
    if not tick and not changed:
        return
    var corner := camera.project_ray_origin(Vector2.ZERO)
    var span_u := camera.project_ray_origin(Vector2(screen_size.x, 0.0)) - corner
    var span_v := camera.project_ray_origin(Vector2(0.0, screen_size.y)) - corner
    var view_size := span_v.length()
    var fade_start = _view_material.get_shader_parameter("cloud_close_fade_range")
    if fade_start == null:
        fade_start = RenderingServer.shader_get_parameter_default(
            _view_material.shader.get_rid(), "cloud_close_fade_range")
    if view_size <= fade_start.x:
        _set_view_enabled(false)
        _view_valid = false
        return
    var band_size := Vector2i(Vector2(screen_size) * cloud_resolution_scale * VIEW_OVERSCAN)
    _view_viewport.size = Vector2i(maxi(band_size.x, 1), maxi(band_size.y, 1) * 2)
    var center := corner + (span_u + span_v) * 0.5
    corner -= (span_u + span_v) * ((VIEW_OVERSCAN - 1.0) * 0.5)
    span_u *= VIEW_OVERSCAN
    span_v *= VIEW_OVERSCAN
    _view_material.set_shader_parameter("cloud_view_corner", corner)
    _view_material.set_shader_parameter("cloud_view_u", span_u)
    _view_material.set_shader_parameter("cloud_view_v", span_v)
    _view_material.set_shader_parameter("cloud_view_direction",
        camera.project_ray_normal(Vector2(screen_size) * 0.5))
    _view_material.set_shader_parameter("cloud_view_camera_height", transform.origin.y)
    _view_material.set_shader_parameter("cloud_view_size", view_size)
    for material in _view_receivers:
        material.set_shader_parameter("cloud_view_center", center)
        material.set_shader_parameter("cloud_view_u", span_u)
        material.set_shader_parameter("cloud_view_v", span_v)
    _view_viewport.render_target_update_mode = SubViewport.UPDATE_ONCE
    _set_view_enabled(true)
    _view_camera = camera
    _view_transform = transform
    _view_camera_size = camera.size
    _view_screen_size = screen_size
    _view_valid = true

func _set_view_enabled(enabled: bool) -> void:
    for material in _view_receivers:
        material.set_shader_parameter("cloud_use_view_map", enabled)

func _ready() -> void:
    # The standalone art preview has inline materials instead of factory copies.
    for mesh in get_parent().find_children("*", "MeshInstance3D", true, false):
        var material := mesh.material_override as ShaderMaterial
        if material == null:
            continue
        var shader_path := material.shader.resource_path
        var is_cloud := shader_path == "res://shaders/cloud_layer.gdshader"
        if is_cloud or shader_path == "res://shaders/low_poly.gdshader":
            material.set_shader_parameter("cloud_weather_origin", weather_origin)
            _bind_clock(material)
            _bind_weather_map(material)
            _bind_sunlight_map(material)
            if is_cloud and not _view_receivers.has(material):
                material.set_shader_parameter("cloud_view_map", _view_viewport.get_texture())
                _view_receivers.append(material)
