class_name SeedProjectile
extends Area

signal strike

export var muzzle_velocity := 15
export var g := Vector3.DOWN * 10

var velocity := Vector3.ZERO


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass  # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta: float) -> void:
	velocity += g * delta
	look_at(transform.origin + velocity.normalized(), Vector3.UP)
	transform.origin += velocity * delta


func _on_Seed_proj_body_entered(_body: PhysicsBody):
	emit_signal("strike", transform.origin)
	queue_free()
