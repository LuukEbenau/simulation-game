extends Node3D

var house_model = preload("res://assets/buildings/house.blend")

func _ready():
	spawn_house()

func spawn_house():
	var house_instance = house_model.instantiate()

	# Adjust the position to place it on top of this node
	# You might need to adjust this offset depending on your model's origin
	house_instance.position = Vector3(0, 0, 0)

	# You can adjust the scale if needed
	# house_instance.scale = Vector3(1, 1, 1)

	add_child(house_instance)

	# Optionally, adjust the house position if needed
	adjust_house_position(house_instance)

func adjust_house_position(house):
	# Find the mesh within the house instance
	var mesh_instance = find_mesh_instance(house)
	if mesh_instance:
		# Get the AABB (Axis-Aligned Bounding Box) of the mesh
		var aabb = mesh_instance.get_aabb()

		# Move the house up by half its height
		house.position.y += aabb.size.y / 2

func find_mesh_instance(node):
	if node is MeshInstance3D:
		return node
	for child in node.get_children():
		var result = find_mesh_instance(child)
		if result:
			return result
	return null
