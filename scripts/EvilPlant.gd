class_name EvilPlant
extends KinematicBody

export (PackedScene) var seed_proj

var player = null
var i := 1
onready var flower_body: Spatial = $Armature
onready var anim_tree: AnimationTree = $AnimationTree2
onready var bullet_spawn_point = get_node(@"Armature/Position3D")


# Called when the node enters the scene tree for the first time.
func _ready():
	set_process(true)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if player:
		flower_body.look_at(player.get_global_transform().origin, Vector3(0, 1, 0))

func _on_DetectArea_body_entered(body: PhysicsBody) -> void:
	if body.is_in_group("player"):
		var state_machine = anim_tree["parameters/playback"]
		state_machine.travel("attack")
		player = body


func _on_DetectArea_body_exited(_body: PhysicsBody) -> void:
	var state_machine = anim_tree["parameters/playback"]
	state_machine.travel("idle")
	player = null


func shoot_shit() -> void:
	if not player:
		pass

	var s: SeedProjectile = seed_proj.instance()
	add_child(s)
	s.global_transform = bullet_spawn_point.global_transform
	s.velocity = -s.transform.basis.z * s.muzzle_velocity
