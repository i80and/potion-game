class_name PlayerCharacter
extends KinematicBody

signal activate_pie
export var speed: float = 20


func get_input() -> Vector3:
	if Input.is_action_just_pressed("activate_pie"):
		emit_signal("activate_pie")

	# Detect up/down/left/right keystate and only move when pressed
	var velocity: Vector3 = Vector3()
	if Input.is_action_pressed("ui_right"):
		velocity.z -= 0.5
		velocity.x += 1

	if Input.is_action_pressed("ui_left"):
		velocity.z += 0.5
		velocity.x -= 1

	if Input.is_action_pressed("ui_down"):
		velocity.z += 1
		velocity.x += 0.5

	if Input.is_action_pressed("ui_up"):
		velocity.z -= 1
		velocity.x -= 0.5

	return velocity.normalized() * speed


func _physics_process(_delta: float) -> void:
	var velocity = get_input()
	move_and_slide(velocity)
	if translation.y < 0.1:
		translation = Vector3(translation.x, 0.1, translation.z)


func launch_potion(target: Vector3, _slice: String, _tier: int) -> void:
	var potion: ThrownPotion = ThrownPotion.new()
	get_parent().add_child(potion)
	potion.global_translate(($CollisionShape as Spatial).global_transform.origin)
	potion.translate(Vector3(0, 2, 0))
	potion.launch(target)
