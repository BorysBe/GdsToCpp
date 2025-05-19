var targeting:Vector2 = Vector2.ZERO:
	set(v):
		targeting = v
		var angle_rad = atan2(targeting.y, targeting.x)
		var angle_deg = rad_to_deg(angle_rad)
		gun.rotation_degrees = angle_deg
		gun.position.x = 8 if targeting.x<0 else -8 if targeting.x>0 else -last_dir_x*8
		gun.flip_v = last_dir_x < 0

var hp = 50:
	set(val):
		hp = val
		update_shader_color()
		create_tween_for_hp()
		check_player_death()

func update_shader_color():
	$outline.material.set_shader_parameter("effect_enabled", true)
	if hp >= 50:
		$outline.material.set_shader_parameter("color", Color(1, 0, 0, 0))
	elif hp < 25:
		$outline.material.set_shader_parameter("color", Color(1, 0, 0, 0.5))
	elif hp < 50:
		$outline.material.set_shader_parameter("color", Color(1, 1, 0, 0.5))

func create_tween_for_hp():
	var tw = get_tree().create_tween()
	tw.tween_property(Hearts.hp, "value", hp, 0.5)

func check_player_death():
	if hp <= 0 and $StateMachine.state != $StateMachine/die:
		$StateMachine.state.change_state.emit(PlayerState.DIE)
		$outline.material.set_shader_parameter("color", Color(1, 0, 0, 0))

var is_armed = false:
	set(value):
		is_armed = value
		post_anim = "_armed" if is_armed else ""
var shurekining = false
var post_anim = ""

func _ready() -> void:
	if Ut.unkillable:
		hp = 99999999
	if Ut.maxammo:
		var index = 0
		for wep in weapons:
			wep.ammo = -1
			index += 1	
	if not Ut.border_over_object:
		$outline.material.set_shader_parameter("effect_enabled", false)
	else:
		$outline.material.set_shader_parameter("color", Color(1, 1, 1, 1))

	if Ut.gamepad_id != 9:
		has_joypad = true
	$cgroup/dupsko.material.set_shader_parameter("is_hit",false)
	body.material.set_shader_parameter("is_hit",false)
	alegs.material.set_shader_parameter("is_hit",false)
	if Ut.is_update_weapons():
		for wep in weapons.size():
			weapons[wep].ammo = Ut.weapons[wep].ammo
		if Ut.is_chechkpoint:
			global_position = Ut.check_point_position
		bunny_hopping_inc = Ut.statuses.bunny_hopping_inc
	if show_states_with_animation:
		legs.visible = true
		$"body-hands".visible = true

	change_weapon(Ut.last_weapon)
	Ut.camera = $Camera2D
	Ut.player = self