class_name Bush1
extends Area


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass
#	set_contact_monitor(true)
#	set_max_contacts_reported(2)


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

func _on_Bush1_body_entered(body):
	$CPUPart_leaves.emitting = true
	$AnimationPlayer.play("bush_shake")


