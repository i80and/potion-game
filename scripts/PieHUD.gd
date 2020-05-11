class_name PieHUD
extends Control

signal pie_released(position, slice, tier)

const TIERS = [62, 100, 142]

const SLICES = [
	"damage",
	"damage-buff",
	"buff",
	"buff-health",
	"health",
	"health-curse",
	"curse",
	"curse-damage"
]

const SLICE_RADIANS: float = (2.0 * PI) / 8.0

const DEAD_ZONE_RADIUS: int = 10

var _selected: Selection = null


class Selection:
	extends Reference
	var slice: String
	var tier: int

	func _init(s: String, t: int).():
		slice = s
		tier = t


static func angle_between(n: float, a: float, b: float) -> bool:
	n = fmod(TAU + fmod(n, TAU), TAU)
	a = fmod(TAU * 10 + a, TAU)
	b = fmod(TAU * 10 + b, TAU)

	if a < b:
		return a <= n && n <= b

	return a <= n || n <= b


func _draw() -> void:
	if _selected == null:
		return

	var r = TIERS[_selected.tier - 1]
	var theta = SLICES.find(_selected.slice) * SLICE_RADIANS
	var cartesian = polar2cartesian(r, -theta)
	draw_circle(cartesian, 15.0, Color(1.0, 1.0, 1.0, 0.8))


func _on_PieHUD_visibility_changed() -> void:
	if visible:
		Engine.time_scale = 0.1
		rect_position = get_global_mouse_position()
	else:
		Engine.time_scale = 1.0
		warp_mouse(Vector2(0, 0))
		if _selected != null:
			emit_signal("pie_released", rect_position, _selected.slice, _selected.tier)
			_selected = null


func _input(_event: InputEvent) -> void:
	if Input.is_action_just_released("activate_pie"):
		visible = false

	var cursor = get_local_mouse_position()
	var r: float = sqrt(pow(cursor.x, 2) + pow(cursor.y, 2))
	var theta: float = wrapf(atan2(-cursor.y, cursor.x), 0, TAU)

	if r <= DEAD_ZONE_RADIUS:
		_selected = null
		update()
		return

	var tier: int = 0
	var delta: float = INF
	for i in range(len(TIERS)):
		var candidate_delta: float = abs(TIERS[i] - r)
		if candidate_delta < delta:
			tier = i + 1
			delta = candidate_delta

	var slice_theta = TAU - (SLICE_RADIANS / 2)
	var slice: String = ""
	for candidate_slice in SLICES:
		if angle_between(theta, slice_theta, wrapf(slice_theta + SLICE_RADIANS, 0, TAU)):
			slice = candidate_slice
			break

		slice_theta = wrapf(slice_theta + SLICE_RADIANS, 0, TAU)

	if slice.empty():
		_selected = null
	else:
		_selected = Selection.new(slice, tier)
		update()
