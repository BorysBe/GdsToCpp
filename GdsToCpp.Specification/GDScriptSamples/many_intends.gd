func _on_hurtbox_area_entered(area: Area2D) -> void:
	if area.is_in_group("dmgarea"):
		handle_damage_area(area)
	elif area.is_in_group("one_shoot"):
		rocket_jump_possible = true
	elif area.is_in_group("bullet"):
		handle_bullet_area()
	elif area.is_in_group("chekpoint"):
		handle_checkpoint_area(area)

func shake_camera_back(rangeX, rangeY):
	camera_shake.apply_noise_shake(randf_range(-rangeX,rangeX),randf_range(-rangeY,rangeY))