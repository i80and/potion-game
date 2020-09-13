extends State
class_name PlayerState
# Base type for the player's state classes. Contains boilerplate code to get
# autocompletion and type hints.

var player: Player
var skin: Mannequiny


func _ready() -> void:
	yield(owner, "ready")
	var owner_player: Player = owner as Player
	assert(owner_player != null)
	player = owner_player
	skin = owner_player.skin
