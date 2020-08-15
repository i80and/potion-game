extends KinematicBody
var Player = null
onready var Cube = $MeshInstance

# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	set_process(true)
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	if Player:
		Cube.look_at(Player.get_global_transform().origin, Vector3(0,1,0))
		print(Player.get_global_transform().origin)


func _on_Area_body_entered(body):
	if body.is_in_group("player"):
		print("detect")
		Player = body


func _on_Area_body_exited(body):
	Player = null
