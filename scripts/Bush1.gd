class_name Bush1
extends Area

func _on_Bush1_body_entered(_body: PhysicsBody):
	$CPUPart_leaves.emitting = true
	$AnimationPlayer.play("bush_shake")
