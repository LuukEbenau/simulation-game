class_name GridMapMain
extends GridMap

var grid_size = 50
#var house_scene = preload("res://assets/buildings/house.blend")
var grass_scene = preload("res://assets/buildings/grass.blend")
var camera:Camera3D
var house_scene = preload("res://assets/buildings/house.blend")
var from = Vector3()
var to = Vector3()
var raycast_visualizer
@export var debug_raycast = false
var collision_level = 1

var meshTileTypeMap = {
	"Grass": {"coefficient": 10, "index": 0},
	"Road": {"coefficient": 1, "index": 2},
	"House": {"coefficient": 10, "index": 1}
}

func _ready():
	raycast_visualizer = get_node("RaycastVisualizer")
	camera = get_viewport().get_camera_3d()

	collision_layer = collision_level
	collision_mask = collision_level

func _input(event):
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if camera == null:
			print("Camera not found")
			return
		print("Camera is called %s" % camera.name)

		var mouse_pos = get_viewport().get_mouse_position()
		from = camera.project_ray_origin(mouse_pos)
		to = from + camera.project_ray_normal(mouse_pos) * 500

		var space_state = get_world_3d().direct_space_state

		var query = PhysicsRayQueryParameters3D.new()
		query.from = from
		query.to = to
		query.collide_with_areas = true
		query.collide_with_bodies = true

		query.collision_mask = collision_level

		var result = space_state.intersect_ray(query)

		print("Raycast result: ", result)
		if result:
			print("Hit something")
			print("Collider: ", result.collider)
			if result.collider == self:
				var cell = local_to_map(result.position)
				print("Hit the GridMap on cell %s" % cell)
				place_house(cell.x, cell.z)
			else:
				print("Hit something else")
		else:
			print("Didn't hit anything")

		if debug_raycast:
			raycast_visualizer.draw_ray(from, to)

func place_house(x, z):
	var house_instance = house_scene.instantiate()
	house_instance.position = map_to_local(Vector3i(x, 0, z)) + Vector3(0,-1.5, 0)
	add_child(house_instance)

