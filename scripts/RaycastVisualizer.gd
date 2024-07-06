extends Node3D

@export var line_color: Color = Color(1, 0, 0) # Red color

func draw_ray(from: Vector3, to: Vector3):
	var mesh = ImmediateMesh.new()
	mesh.surface_begin(Mesh.PRIMITIVE_LINES)
	mesh.surface_set_color(line_color)
	mesh.surface_add_vertex(from)
	mesh.surface_add_vertex(to)
	mesh.surface_end()

	var mesh_instance = $MeshInstance3D
	mesh_instance.mesh = mesh
