tool
class_name CameraRig
extends Spatial
# Accessor class that gives the nodes in the scene access the player or some
# frequently used nodes in the scene itself.

var player: KinematicBody
var zoom := 0.5 setget set_zoom

onready var camera: InterpolatedCamera = $InterpolatedCamera
onready var spring_arm: ZoomableSpringArm = $SpringArm
onready var aim_ray: RayCast = $InterpolatedCamera/AimRay
onready var _position_start: Vector3 = translation


func _ready() -> void:
	set_as_toplevel(true)
	camera.set_orthogonal(camera.size, -100, 100)
	yield(owner, "ready")
	player = owner


func _get_configuration_warning() -> String:
	return "Missing player node" if not player else ""


func set_zoom(value: float) -> void:
	zoom = clamp(value, 0.0, 1.0)
	if not spring_arm:
		yield(spring_arm, "ready")
	spring_arm.zoom = zoom
