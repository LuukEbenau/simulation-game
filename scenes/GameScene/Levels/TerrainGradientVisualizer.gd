extends Node3D

var terrainGradients: Dictionary
var cellSize: Vector2
var colorRamp: Gradient
var visualHeight: float = 20.0
var transparency:float = 0.25
func _ready():
    colorRamp = create_color_ramp()

func set_gradients(gradients: Dictionary, cell_size: Vector2):
    terrainGradients = gradients
    cellSize = cell_size
    create_visualization()

func create_visualization():
    for cell in terrainGradients:
        var angle = terrainGradients[cell]
        var color = get_color_for_angle(angle)
        create_cell_visual(cell, color)

func create_cell_visual(cell: Vector2i, color: Color):
    var mesh_instance = MeshInstance3D.new()
    var plane_mesh = PlaneMesh.new()
    plane_mesh.size = Vector2(cellSize.x, cellSize.y)
    mesh_instance.mesh = plane_mesh

    var material = StandardMaterial3D.new()
    material.albedo_color = color
    material.flags_transparent = true
    material.alpha_scissor_threshold = 0.5
    material.alpha_antialiasing_mode = StandardMaterial3D.ALPHA_ANTIALIASING_ALPHA_TO_COVERAGE
    mesh_instance.set_surface_override_material(0, material)

    mesh_instance.position = Vector3(cell.x * cellSize.x, visualHeight, cell.y * cellSize.y)
    add_child(mesh_instance)

func get_color_for_angle(angle: float) -> Color:
    angle = clamp(angle, 0, 90)
    var t = angle / 90.0
    var color = colorRamp.sample(t)
    color.a = transparency  # Set transparency
    return color

func create_color_ramp() -> Gradient:
    var gradient = Gradient.new()
    gradient.set_color(0, Color.GREEN)
    gradient.add_point(0.5, Color.YELLOW)
    gradient.set_color(1, Color.RED)
    return gradient
