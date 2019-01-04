extends RigidBody

const GRAVITY: float = 9.80665

signal detonation(position)

func solve_ballistic_arc_lateral(proj_pos: Vector3, proj_speed: float, target: Vector3, force: bool) -> Vector3:
    assert proj_pos != target && proj_speed > 0
    var diff: Vector3 = target - proj_pos
    var diffXZ: Vector3 = Vector3(diff.x, 0, diff.z)
    var groundDist: float = diffXZ.length()
    var groundDir: Vector3 = diffXZ.normalized()

    var speed2: float = proj_speed * proj_speed
    var speed4: float = proj_speed * proj_speed*proj_speed * proj_speed
    var y: float = diff.y
    var x: float = groundDist
    var gx: float = GRAVITY * x

    var root: float = speed4 - GRAVITY * ((GRAVITY * x * x) + (2 * y * speed2))

    # No solution
    if root < 0:
        if not force:
            return Vector3()
        return groundDir * proj_speed + Vector3(0, 1, 0) * sin(PI / 4.0) * proj_speed

    root = sqrt(root)

    var lowAng: float = atan2(speed2 - root, gx)
    var highAng: float = atan2(speed2 + root, gx)
    var numSolutions: int = 2 if lowAng != highAng else 1

    return groundDir * cos(lowAng) * proj_speed + Vector3(0, 1, 0) * sin(lowAng) * proj_speed

func throw(target: Vector3) -> void:
    apply_torque_impulse(Vector3(randf(), randf(), randf()))
    print(global_transform.origin.round(), target.round())
    var vec := solve_ballistic_arc_lateral(global_transform.origin, 10, target, true)
    apply_central_impulse(vec)

func _onhit(node: Node) -> void:
    self.get_parent().remove_child(self)

func _ready() -> void:
    connect("body_entered", self, "_onhit")