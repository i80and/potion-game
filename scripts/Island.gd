tool
class_name Island
extends Spatial

const TREES: Array = [
	preload("res://scenes/Tree1.tscn"),
	preload("res://scenes/Tree2.tscn"),
	preload("res://scenes/Maple.tscn")
]


# Called when the node enters the scene tree for the first time.
func _ready():
	var i: int = 0
	for child in get_children():
		if child is Spatial:
			if child.name.begins_with("tree"):
				var child_tree = TREES[i % TREES.size()].instance()
				child.add_child(child_tree)
				i += 1
