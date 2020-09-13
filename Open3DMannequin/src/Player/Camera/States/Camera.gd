extends CameraState
# Parent state for all camera based states for the Camera. Handles input based on
# the mouse or the gamepad. The camera's movement depends on the active child state.
# Holds shared logic between all states that move or rotate the camera.

const ZOOM_STEP := 0.1

const ANGLE_X_MIN := -PI / 4
const ANGLE_X_MAX := PI / 3

export var is_y_inverted := false
export var fov_default := 70.0
export var deadzone := PI / 10
export var sensitivity_gamepad := Vector2(2.5, 2.5)
export var sensitivity_mouse := Vector2(0.1, 0.1)

var _input_relative := Vector2.ZERO


func process(delta: float) -> void:
	camera_rig.global_transform.origin = (
		camera_rig.player.global_transform.origin
		+ camera_rig._position_start
	)

func unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("zoom_in"):
		camera_rig.zoom += ZOOM_STEP
	elif event.is_action_pressed("zoom_out"):
		camera_rig.zoom -= ZOOM_STEP
	elif event is InputEventMouseMotion and Input.get_mouse_mode() == Input.MOUSE_MODE_CAPTURED:
		_input_relative += (event as InputEventMouseMotion).get_relative()

# Returns the direction of the camera movement from the player
static func get_look_direction() -> Vector2:
	return Vector2(Input.get_action_strength("look_right") - Input.get_action_strength("look_left"), Input.get_action_strength("look_up") - Input.get_action_strength("look_down")).normalized()

# Returns the move direction of the character controlled by the player
static func get_move_direction() -> Vector3:
	return Vector3(
		Input.get_action_strength("move_right") - Input.get_action_strength("move_left"),
		0,
		Input.get_action_strength("move_back") - Input.get_action_strength("move_front")
	)
