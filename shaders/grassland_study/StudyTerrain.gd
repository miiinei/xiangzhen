extends RefCounted
## A fixed landscape composition. Distances and heights are in kilometres.
## Geometry, surface normals, vegetation and the scale grid share these fields.

const SPAN := 40.0
const SEGMENTS := 192
const FIELD_SIZE := 1024

var _land := FastNoiseLite.new()
var _patch := FastNoiseLite.new()
var _tuft := FastNoiseLite.new()

func _init() -> void:
    for noise in [_land, _patch, _tuft]:
        noise.noise_type = FastNoiseLite.TYPE_SIMPLEX_SMOOTH
        noise.fractal_type = FastNoiseLite.FRACTAL_NONE
        noise.frequency = 1.0
    _land.seed = 2571
    _patch.seed = 1067
    _tuft.seed = 7813

func height_at(p: Vector2) -> float:
    var u := p.x * 0.82 + p.y * 0.572
    var v := -p.x * 0.572 + p.y * 0.82
    var valley := v + sin(u * 0.48) * 0.75 - 0.4
    # A winding shallow swale separates two long, gentle shoulders.
    var height := 0.18 + sin(u * 0.66 + sin(v * 0.31) * 0.35) * 0.32
    height += sin(v * 0.54 + u * 0.19) * 0.13
    height -= exp(-valley * valley / 1.3) * 0.18
    height += _land.get_noise_2d(p.x * 0.8, p.y * 0.8) * 0.026
    return height

func vegetation_at(p: Vector2) -> Color:
    var u := p.x * 0.82 + p.y * 0.572
    var v := -p.x * 0.572 + p.y * 0.82
    var valley := v + sin(u * 0.48) * 0.75 - 0.4
    var moisture := exp(-valley * valley / 2.8)
    var patches := _patch.get_noise_2d(p.x * 0.8, p.y * 0.8)
    var broad := _patch.get_noise_2d(p.x * 0.30 + 13.0, p.y * 0.30)
    var cover := clampf(0.44 + moisture * 0.39 + broad * 0.24 + patches * 0.14, 0.2, 0.95)
    var height := clampf(cover * 0.68 + moisture * 0.22 + patches * 0.15, 0.16, 0.94)
    # B: spatial variation in leaf orientation and recovery; A: tuft coverage.
    var orientation := _patch.get_noise_2d(p.x * 4.2 + 31.0, p.y * 4.2) * 0.5 + 0.5
    var tufts := _tuft.get_noise_2d(p.x * 13.0, p.y * 13.0) * 0.5 + 0.5
    return Color(cover, height, orientation, tufts)

func normal_at(p: Vector2, include_tufts: bool = false) -> Vector3:
    var step := 0.012 if include_tufts else 0.025
    var dx := Vector2(step, 0.0)
    var dz := Vector2(0.0, step)
    var x := _surface_height(p + dx, include_tufts) - _surface_height(p - dx, include_tufts)
    var z := _surface_height(p + dz, include_tufts) - _surface_height(p - dz, include_tufts)
    return Vector3(-x / (2.0 * step), 1.0, -z / (2.0 * step)).normalized()

func _surface_height(p: Vector2, include_tufts: bool) -> float:
    var height := height_at(p)
    if include_tufts:
        # Aggregate canopy relief at tens of metres, rather than individual blades.
        var cover := vegetation_at(p).r
        height += _tuft.get_noise_2d(p.x * 13.0, p.y * 13.0) * 0.00045 * cover
    return height

func make_mesh() -> ArrayMesh:
    var vertices := PackedVector3Array()
    var normals := PackedVector3Array()
    var indices := PackedInt32Array()
    for z in range(SEGMENTS + 1):
        for x in range(SEGMENTS + 1):
            var p := Vector2(float(x) / SEGMENTS - 0.5, float(z) / SEGMENTS - 0.5) * SPAN
            vertices.append(Vector3(p.x, height_at(p), p.y))
            normals.append(normal_at(p))
    for z in range(SEGMENTS):
        for x in range(SEGMENTS):
            var a := z * (SEGMENTS + 1) + x
            var b := a + 1
            var c := a + SEGMENTS + 1
            var d := c + 1
            indices.append_array(PackedInt32Array([a, b, c, b, d, c]))
    var arrays := []
    arrays.resize(Mesh.ARRAY_MAX)
    arrays[Mesh.ARRAY_VERTEX] = vertices
    arrays[Mesh.ARRAY_NORMAL] = normals
    arrays[Mesh.ARRAY_INDEX] = indices
    var mesh := ArrayMesh.new()
    mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, arrays)
    return mesh

func bake_fields() -> Array[Image]:
    var cover := Image.create(FIELD_SIZE, FIELD_SIZE, false, Image.FORMAT_RGBA8)
    var surface := Image.create(FIELD_SIZE, FIELD_SIZE, false, Image.FORMAT_RGB8)
    for z in range(FIELD_SIZE):
        for x in range(FIELD_SIZE):
            var p := (Vector2(x + 0.5, z + 0.5) / FIELD_SIZE - Vector2(0.5, 0.5)) * SPAN
            cover.set_pixel(x, z, vegetation_at(p))
            var n := normal_at(p, true)
            surface.set_pixel(x, z, Color(n.x * 0.5 + 0.5, n.y * 0.5 + 0.5, n.z * 0.5 + 0.5))
    return [cover, surface]
