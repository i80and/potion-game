extends CameraState
# Rotates the camera around the character, delegating all the work to its parent state.


func process(delta: float) -> void:
	_parent.process(delta)
