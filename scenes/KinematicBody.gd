extends KinematicBody

const GRAVITY := 9.81
const SPEED: = 250

func get_input() -> Vector3:
    # Detect up/down/left/right keystate and only move when pressed
    var velocity := Vector3()
    if Input.is_action_pressed('ui_right'):
        velocity.x += 1
    if Input.is_action_pressed('ui_left'):
        velocity.x -= 1
    if Input.is_action_pressed('ui_down'):
        velocity.z += 1
    if Input.is_action_pressed('ui_up'):
        velocity.z -= 1
    return velocity.normalized() * SPEED

func _physics_process(delta) -> void:
    var velocity := get_input()
    velocity.y = -GRAVITY
    move_and_collide(velocity)