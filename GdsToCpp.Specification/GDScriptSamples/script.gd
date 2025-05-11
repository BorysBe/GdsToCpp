extends CharacterBody2D
class_name Player

@export var speed := 320.0
@export var jump_force := 1000.0
@export var show_states_with_animation = false
@onready var cam: Camera2D = $Camera2D
@onready var asprite = $old
@onready var col:CollisionShape2D = $CollisionShape2D
@onready var gun = $gun
@onready var ground_detector: RayCast2D = $RayCast2D
@onready var legs: Label = $legs
@onready var body: AnimatedSprite2D = $cgroup/body
@onready var camera_shake:Camera_Shake = $camera_shake

var bullet = preload("res://scenes/player/bullets/bullet.tscn")

@onready var weapons = [
    {
        "name": "pistol" ,
        "bullet": preload("res://scenes/player/bullets/bullet.tscn"),
        "icon" : preload("res://assets/gun.png"),
        "ammo" : -1,
        "cd" : 0.3,
        "shift" : func (): return Vector2(0, randi_range(-2,2))
    },
]

var map:PackedScene	

func save_map():
	map = PackedScene.new()
	map.pack(get_parent())
	ResourceSaver.save(map,"user://Checkpoint.tscn")

func load():
	var scene = ResourceLoader.load("user://Checkpoint.tscn")
	get_tree().change_scene_to_packed(scene)

func shake_camera_random(rangeX, rangeY):
	#cam.offset.x = randf_range(-rangeX,rangeX)
	#cam.offset.y = randf_range(-rangeY,rangeY)
	camera_shake.apply_noise_shake(randf_range(-rangeX,rangeX),randf_range(-rangeY,rangeY))
	#$shake_cam.start()
	
func shake_camera_back(rangeX, rangeY):
	camera_shake.apply_noise_shake(randf_range(-rangeX,rangeX),randf_range(-rangeY,rangeY))
	#cam.offset.x = rangeX
	#cam.offset.y = rangeY
	#$shake_cam.start()

func _on_shake_cam_timeout() -> void:
	cam.offset.x = 0
	cam.offset.y = 0
	
func is_on_platform():
	return $RayCast2D.is_colliding() or $RayCast2D2.is_colliding()
