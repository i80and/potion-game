class_name evilplant_04
extends KinematicBody

# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var player = null
onready var Flower_body: Spatial = $Armature
onready var Anim_tree: AnimationTree = $AnimationTree2
onready var bullet_spawn_point = get_node(@"Armature/Position3D")
var i := 1
export (PackedScene) var Seed_proj

# Called when the node enters the scene tree for the first time.
func _ready():
	set_process(true)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if player:
		Flower_body.look_at(player.get_global_transform().origin, Vector3(0,1,0))
#		var s = Seed_proj.instance()
#		add_child(s)
#		s.global_transform = $Position3D.global_transform
#		s.velocity = -s.transform.basis.z * s.muzzle_velocity
#			#print(angle)
#			#print(player_dir)
#	else:
#		pass

func _on_DetectArea_body_entered(body):
	if body.is_in_group("player"):
		var state_machine = Anim_tree["parameters/playback"]
		state_machine.travel("attack")
		player = body

func _on_DetectArea_body_exited(body):
	var state_machine = Anim_tree["parameters/playback"]
	state_machine.travel("idle")
	player = null

func shoot_shit():
	if not player:
		pass
		
	var s = Seed_proj.instance()
	add_child(s)
	s.global_transform = bullet_spawn_point.global_transform
	s.velocity = -s.transform.basis.z * s.muzzle_velocity
