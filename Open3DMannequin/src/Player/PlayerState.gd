class_name PlayerState
extends State

var player: Player
var skin: Mannequiny


func _ready() -> void:
	yield(owner, "ready")
	var owner_player: Player = owner as Player
	assert(owner_player != null)
	player = owner_player
	skin = owner_player.skin
