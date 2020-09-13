class_name ThrownPotion
extends RigidBody

const GRAVITY: float = 9.80665

static func solve_ballistic_arc_lateral(
		proj_pos: Vector3, proj_speed: float, target: Vector3, force: bool
) -> Vector3:
	assert(proj_pos != target && proj_speed > 0)
	var diff: Vector3 = target - proj_pos
	var diff_xz: Vector3 = Vector3(diff.x, 0, diff.z)
	var ground_dist: float = diff_xz.length()
	var ground_dir: Vector3 = diff_xz.normalized()

	var speed2 = proj_speed * proj_speed
	var speed4 = proj_speed * proj_speed * proj_speed * proj_speed
	var y = diff.y
	var x = ground_dist
	var gx: float = GRAVITY * x

	var root: float = speed4 - GRAVITY * ((GRAVITY * x * x) + (2 * y * speed2))

	# No solution
	if root < 0:
		if ! force:
			return Vector3()

		return ground_dir * proj_speed + Vector3(0, 1, 0) * sin(PI / 4.0) * proj_speed

	root = sqrt(root)

	var low_ang: float = atan2(speed2 - root, gx)
	# var high_ang = atan2(speed2 + root, gx)
	# var numSolutions = 2 if (low_ang != high_ang) else 1

	return ground_dir * cos(low_ang) * proj_speed + Vector3(0, 1, 0) * sin(low_ang) * proj_speed


func launch(target: Vector3) -> void:
	apply_torque_impulse(Vector3(randf(), randf(), randf()))
	var vec = solve_ballistic_arc_lateral(global_transform.origin, 10, target, true)
	apply_central_impulse(vec)


func _on_hit(_node: Node) -> void:
	get_parent().remove_child(self)
