extends Node3D

const Terrain = preload("res://shaders/grassland_study/StudyTerrain.gd")

var terrain := Terrain.new()
var grass_material: ShaderMaterial
var wind_time := 0.0
var wind_paused := false
var _focus := Vector3.ZERO
var _dragging := false

@onready var camera: Camera3D = $Camera3D
@onready var weather = $CloudWeather
@onready var grid: MeshInstance3D = $ScaleGrid
@onready var caption: Label = $CanvasLayer/Caption

func _ready() -> void:
    grass_material = preload("res://shaders/grassland_study/grassland_study.tres").duplicate()
    $Ground.mesh = terrain.make_mesh()
    $Ground.material_override = grass_material
    grass_material.set_shader_parameter("terrain_origin", Vector2(global_position.x, global_position.z))
    # This local material receives the existing weather caches; shared templates stay intact.
    grass_material.set_shader_parameter("cloud_weather_origin", weather.weather_origin)
    weather._bind_clock(grass_material)
    weather._bind_weather_map(grass_material)
    weather._bind_sunlight_map(grass_material)
    _make_grid()
    _place_camera()
    _update_caption()

func _process(delta: float) -> void:
    if not wind_paused:
        wind_time += delta
        grass_material.set_shader_parameter("wind_time", wind_time)

func _unhandled_input(event: InputEvent) -> void:
    if event is InputEventMouseButton:
        if event.button_index == MOUSE_BUTTON_WHEEL_UP and event.pressed:
            camera.size = maxf(8.0, camera.size / 1.12)
            _place_camera()
        elif event.button_index == MOUSE_BUTTON_WHEEL_DOWN and event.pressed:
            camera.size = minf(40.0, camera.size * 1.12)
            _place_camera()
        elif event.button_index == MOUSE_BUTTON_RIGHT:
            _dragging = event.pressed
    elif event is InputEventMouseMotion and _dragging:
        var factor := camera.size / get_viewport().get_visible_rect().size.y
        _focus += (-camera.global_basis.x * event.relative.x
            + Vector3(-1.0, 0.0, -1.0).normalized() * event.relative.y / sin(deg_to_rad(35.264))) * factor
        _focus.x = clampf(_focus.x, -7.0, 7.0)
        _focus.z = clampf(_focus.z, -7.0, 7.0)
        _place_camera()
    elif event is InputEventKey and event.pressed and not event.echo:
        match event.keycode:
            KEY_G:
                grid.visible = not grid.visible
            KEY_SPACE:
                wind_paused = not wind_paused
            KEY_1:
                camera.size = 8.0
            KEY_2:
                camera.size = 20.0
            KEY_3:
                camera.size = 40.0
            KEY_R:
                camera.size = 12.0
                _focus = Vector3.ZERO
        _place_camera()
        _update_caption()

func _place_camera() -> void:
    var target := Vector3(_focus.x, terrain.height_at(Vector2(_focus.x, _focus.z)), _focus.z)
    camera.position = target + Vector3.ONE * (camera.size / sqrt(3.0))
    camera.look_at(to_global(target))

func _update_caption() -> void:
    caption.text = "雨季草原 · 微地形样区\n滚轮缩放 / 右键平移 / G 公里六边格 / 空格%s / R 复位" % ("继续风动" if wind_paused else "暂停风动")

func _make_grid() -> void:
    var mesh := ImmediateMesh.new()
    mesh.surface_begin(Mesh.PRIMITIVE_LINES)
    # Six-sided cells with a one-kilometre edge. Sample each edge onto the relief.
    for row in range(-5, 6):
        for col in range(-5, 6):
            var centre := Vector2(col * 1.5, (row + 0.5 * posmod(col, 2)) * sqrt(3.0))
            for side in range(3):
                var a := centre + Vector2(cos(side * PI / 3.0), sin(side * PI / 3.0))
                var b := centre + Vector2(cos((side + 1) * PI / 3.0), sin((side + 1) * PI / 3.0))
                for segment in range(16):
                    for t in [float(segment) / 16.0, float(segment + 1) / 16.0]:
                        var p := a.lerp(b, t)
                        mesh.surface_add_vertex(Vector3(p.x, terrain.height_at(p) + 0.008, p.y))
    mesh.surface_end()
    grid.mesh = mesh
