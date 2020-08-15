class_name evilplant_04
extends KinematicBody

# Declare member variables here. Examples:
# var a = 2
# var b = "text"
var EnemyToPlayer
var DetectDistance = 5
var Ray = null
var Player = null
onready var Flower_body = $Armature
onready var Anim_tree = $AnimationTree
var i = 1

# Called when the node enters the scene tree for the first time.
func _ready():
	Ray = get_node("RayCast")
	set_process(true)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Player:
		Flower_body.look_at(Player.get_global_transform().origin, Vector3(0,1,0))
		if i < 5:
			i+=1
		else:
			i = 1
			#print(angle)
			#print(player_dir)
	else:
		pass

func _on_DetectArea_body_entered(body):
	if body.is_in_group("player"):
		Anim_tree.set("parameters/Wakeup/blend_amount", 1.0)
		Player = body

func _on_DetectArea_body_exited(body):
	Anim_tree.set("parameters/Wakeup/blend_amount", 0)
	Player = null
