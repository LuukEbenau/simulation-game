class_name MapGridManager
extends Node3D

#region initialization

@export var terrain: Node3D

@onready var gradient_visualizer = $TerrainGradientVisualizer

var cell_size = Vector2i(1,1)
var raycast_start_height = 50.0
var raycast_length = 100.0
var directions = [
    Vector2i(1, 0), Vector2i(-1, 0), Vector2i(0, 1), Vector2i(0, -1)
]

var terrainGradients:Dictionary

#endregion initialization

#region node events
func _ready():
    var grid: Dictionary = map_terrain(terrain)
    print_terrain_info(grid)

    terrainGradients = calculate_terrain_gradients(grid)

    gradient_visualizer.position = Vector3(terrain.global_transform.origin.x,gradient_visualizer.position.y, terrain.global_transform.origin.z)
    #gradient_visualizer.scale = Vector2(your_scale_factor, your_scale_factor)
    gradient_visualizer.set_gradients(terrainGradients, Vector2(cell_size.x, cell_size.y))

#endregion

#region gradient calculation
func calculate_terrain_gradients(grid: Dictionary):
    var slope_gradients = {}
    for cell in grid:
        var slope = get_cell_slope(cell)
        slope_gradients[cell] = slope

    return slope_gradients

func get_cell_slope(pos: Vector2i) -> float:
    # Sample terrain heights at all four cell corners
    var h1 = check_height_at_position(pos)
    var h2 = check_height_at_position(pos + Vector2i(cell_size.x, 0))
    var h3 = check_height_at_position(pos + Vector2i(0, cell_size.y))
    var h4 = check_height_at_position(pos + Vector2i(cell_size.x, cell_size.y))

    # Calculate slopes in all directions
    var slope_x1 = abs((h2 - h1) / cell_size.x)
    var slope_x2 = abs((h4 - h3) / cell_size.x)
    var slope_z1 = abs((h3 - h1) / cell_size.y)
    var slope_z2 = abs((h4 - h2) / cell_size.y)
    var slope_diagonal = abs((h4 - h1) / (sqrt(cell_size.x * cell_size.x + cell_size.y * cell_size.y)))

    # Use the steepest slope
    var max_slope = max(max(max(slope_x1, slope_x2), max(slope_z1, slope_z2)), slope_diagonal)

    # Convert slope to angle in degrees
    var angle = rad_to_deg(atan(max_slope))

    print(angle)
    return angle

func check_height_at_position(pos: Vector2i) -> float:
    var space_state = get_world_3d().direct_space_state
    var start = Vector3(pos.x, raycast_start_height, pos.y)
    var end = start + Vector3.DOWN * raycast_length
    var query = PhysicsRayQueryParameters3D.create(start, end)
    var result = space_state.intersect_ray(query)
    if result:
        var intersection_point = result["position"]
        var length_of_ray = start.distance_to(intersection_point)
        return raycast_start_height - length_of_ray
    else:
        # The ray didn't hit anything, we're outside the terrain
        return -1

#endregion

#region terrain mapping
func map_terrain(terrain: Node3D):
    var to_check: Array[Vector2i] = []
    var mapped_cells = {}

    var center = Vector2i(int(terrain.global_transform.origin.x), int(terrain.global_transform.origin.z))
    to_check.append(center)

    while not to_check.is_empty():
        var current = to_check.pop_front()
        if not mapped_cells.has(current):
            if check_cell(current):
                mapped_cells[current] = true
                for dir in directions:
                    to_check.append(current + dir * cell_size)

    print("Mapping complete. Total cells mapped: ", mapped_cells.size())
    return mapped_cells



func check_cell(cell: Vector2i) -> bool:
    var space_state = get_world_3d().direct_space_state
    var start = Vector3(cell.x, raycast_start_height, cell.y)
    var end = start + Vector3.DOWN * raycast_length

    var query = PhysicsRayQueryParameters3D.create(start, end)
    var result = space_state.intersect_ray(query)

    if result:
        # The ray hit something, presumably the terrain
        return true
    else:
        # The ray didn't hit anything, we're outside the terrain
        return false
#endregion


#region diagnostics
func get_terrain_size(grid: Dictionary) -> Vector2:
    var min_x = INF
    var max_x = -INF
    var min_z = INF
    var max_z = -INF

    for cell in grid.keys():
        min_x = min(min_x, cell.x)
        max_x = max(max_x, cell.x)
        min_z = min(min_z, cell.y)
        max_z = max(max_z, cell.y)

    return Vector2i(max_x - min_x, max_z - min_z) * cell_size

# Call this function to get information about the mapped terrain
func print_terrain_info(grid: Dictionary):
    var size = get_terrain_size(grid)
    print("Approximate terrain size: ", size)
    print("Total cells: ", grid.size())
    print("Approximate area: ", size.x * size.y, " square units")

#endregion diagnostics
