extends Spatial

#   10 units per map pixel
# * 16 map pixels per chunk
# = 160 units per chunk
const UNITS_PER_PIXEL: int = 5
const CHUNK_PIXELS: int = 16
const UNITS_PER_CHUNK: int = CHUNK_PIXELS * UNITS_PER_PIXEL

const DIRECTIONS := PoolVector2Array([
    Vector2(-1, 1), Vector2(0, 1), Vector2(1, 1),
    Vector2(-1, 0), Vector2(0, 0), Vector2(1, 0),
    Vector2(-1, -1), Vector2(0, -1), Vector2(1, -1),
])

class Chunk extends Spatial:
    const TREE1_SCENE := preload("res://scenes/Tree1.tscn")
    const TREE2_SCENE := preload("res://scenes/Tree2.tscn")
    const MAPLE_SCENE := preload("res://scenes/Maple.tscn")
    const ROCK1_SCENE := preload("res://scenes/LowPolyRock1.tscn")
    const ROCK2_SCENE := preload("res://scenes/LowPolyRock2.tscn")
    const CHARACTER_SCENE := preload("res://scenes/PlayerCharacter.tscn")
    
    const TREES: Array = [
        TREE1_SCENE,
        TREE2_SCENE,
        MAPLE_SCENE
    ]
    
    var _chunk_pos: Vector2

    func _init(chunk_pos: Vector2, image: Image).() -> void:
        _chunk_pos = chunk_pos

        seed(hash(chunk_pos))
        randi()

        var material: SpatialMaterial = SpatialMaterial.new()
        material.vertex_color_use_as_albedo = true
        var mesh: QuadMesh = QuadMesh.new()
        mesh.material = material
        mesh.size = Vector2(UNITS_PER_PIXEL, UNITS_PER_PIXEL)
        var ground_multimesh: MultiMesh = MultiMesh.new()
        var instance_count: int = CHUNK_PIXELS * CHUNK_PIXELS
        ground_multimesh.color_format = MultiMesh.COLOR_8BIT
        ground_multimesh.transform_format = MultiMesh.TRANSFORM_3D
        ground_multimesh.instance_count = instance_count
        ground_multimesh.mesh = mesh

        var data: PoolByteArray = image.get_data()
        var i: int = 0
        var offset: int = 0
        for y in range(CHUNK_PIXELS):
            for x in range(CHUNK_PIXELS):
                var r: int = data[offset]
                var g: int = data[offset + 1]
                var b: int = data[offset + 2]
                if g == 200 or g == 209:
                    var scene: PackedScene = TREES[rand_range(0, len(TREES))]
                    add_child(add_node(scene, Vector2(x, y)))

                var color: Color = Color8(r, g, b)
                ground_multimesh.set_instance_color(i, color)
                var transform: Transform = Transform().rotated(Vector3(1, 0, 0), deg2rad(-90)).translated(
                    Vector3(x * UNITS_PER_PIXEL, -y * UNITS_PER_PIXEL, 0)
                )
                ground_multimesh.set_instance_transform(i, transform)
                offset += 3
                i += 1

        var ground_mesh_instance: MultiMeshInstance = MultiMeshInstance.new()
        ground_mesh_instance.multimesh = ground_multimesh
        add_child(ground_mesh_instance)
        var collision_body := StaticBody.new()
        var collision_shape := CollisionShape.new()
        var shape := BoxShape.new()
        shape.extents = Vector3(UNITS_PER_CHUNK, 1, UNITS_PER_CHUNK)
        collision_shape.shape = shape
        collision_body.translate(Vector3(0, -1, 0))
        collision_body.add_child(collision_shape)
        add_child(collision_body)
        
        _place_cosmetics()

    func add_node(scene: PackedScene, pos: Vector2) -> Node:
        var node: Spatial = scene.instance()
        node.translation = Vector3(pos.x, 0, pos.y) * UNITS_PER_PIXEL
        node.rotation = Vector3(0, rand_range(0, TAU), 0)
        var scale_y: float = rand_range(0.8, 1.2)
        var scale_radius: float = rand_range(0.8, 1.2)
        node.scale = Vector3(scale_radius, scale_y, scale_radius)
        return node

    func _place_cosmetics() -> void:
        var n_rocks: float = round(rand_range(0, 50))
        for i in range(n_rocks):
            var rock: Spatial = ROCK1_SCENE.instance() if randf() > 0.5 else ROCK2_SCENE.instance()
            var x: float = rand_range(0, UNITS_PER_CHUNK)
            var y: float = rand_range(0, UNITS_PER_CHUNK)
            rock.translation = Vector3(x, 0, y)
            rock.rotation = Vector3(0, rand_range(0, TAU), 0)
            add_child(rock)

var _image: Image
var _chunks: Dictionary
var _node: Node
var _worker: Thread = Thread.new()

func _init(image: Image, node: Node) -> void:
    self._node = node
    self._image = image
    assert _image.get_format() == Image.FORMAT_RGB8
    assert self._image.get_width() % CHUNK_PIXELS == 0
    assert self._image.get_height() % CHUNK_PIXELS == 0

func _worker_load_chunk(candidate_positions: PoolVector2Array) -> Array:
    var chunks := []
    for candidate in candidate_positions:
        var rect := Rect2(candidate.x * CHUNK_PIXELS, candidate.y * CHUNK_PIXELS, CHUNK_PIXELS, CHUNK_PIXELS)
        var chunk := Chunk.new(candidate, self._image.get_rect(rect))
        chunk.translate(Vector3(
            candidate.x * UNITS_PER_CHUNK,
            0,
            candidate.y * UNITS_PER_CHUNK))
        chunks.append(chunk)

    call_deferred("_worker_finished")
    return chunks

func _worker_finished() -> void:
    var chunks: Array = _worker.wait_to_finish()
    for chunk in chunks:
        _chunks[chunk._chunk_pos] = chunk
        _node.add_child(chunk)

func _gc(camera_chunk_pos: Vector2) -> void:
    for chunk_pos in _chunks.keys():
        if abs(chunk_pos.x - camera_chunk_pos.x) > 1 or \
           abs(chunk_pos.y - camera_chunk_pos.y) > 1:
            var node: Node = _chunks[chunk_pos]
            _chunks.erase(chunk_pos)
            _node.remove_child(node)

func load_chunk(chunk_pos: Vector2) -> void:
    if _worker.is_active():
        return

    if len(_chunks) > 12:
        _gc(chunk_pos)

    var needed := PoolVector2Array()
    for dir in DIRECTIONS:
        var candidate: Vector2 = chunk_pos + dir
        if candidate.x < 0 or candidate.y < 0:
            continue
        if _chunks.has(candidate):
            continue
        needed.append(candidate)

    if len(needed) == 0:
        return

    assert _worker.start(self, "_worker_load_chunk", needed, Thread.PRIORITY_LOW) == OK