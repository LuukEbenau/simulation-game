class_name AstarPathfinder

var astar_grid: AStarGrid2D
var mapGrid: GridMapMain

var unpassableCellValue = 1

var offset_x: int
var offset_z: int
func _init(mapGrid: GridMapMain):
	print("initializing pathfinder")
	self.mapGrid = mapGrid
	self.astar_grid = AStarGrid2D.new()
	var wh = calculate_width_height(mapGrid.get_used_cells())
	self.offset_x = wh.get("min_x")
	self.offset_z = wh.get("min_z")
	var width = wh.get("width")
	var height = wh.get("height")
	var region = Rect2i(0, 0, width, height)  # Start at (0, 0)
	astar_grid.region = region
	#astar_grid.cell_size = Vector2i(2,2)
	astar_grid.update()
	print("Region selected of %s" % region)
	astar_grid.fill_solid_region(region)
	astar_grid = assign_coefficients_to_grid(astar_grid, mapGrid, wh.get("min_x"), wh.get("min_z"))

func calculate_width_height(used_cells: Array) -> Dictionary:
	var min_x = INF
	var max_x = -INF
	var min_z = INF
	var max_z = -INF
	for cell in used_cells:
		min_x = min(cell.x, min_x)
		max_x = max(cell.x, max_x)
		min_z = min(cell.z, min_z)
		max_z = max(cell.z, max_z)
	var width = max_x - min_x + 1
	var height = max_z - min_z + 1
	print("calculated map region to be min_x: %s, min_z: %s, max_x: %s, max_z: %s" % [min_x,min_z,max_x,max_z])
	return {"width": width, "height": height, "min_x": min_x, "min_z": min_z}

func assign_coefficients_to_grid(astar_grid: AStarGrid2D, mapGrid: GridMapMain, offset_x: int, offset_z: int):
	var gridCells = mapGrid.get_used_cells()
	for gridCell in gridCells:
		var itemId = mapGrid.get_cell_item(gridCell)
		var itemName = mapGrid.mesh_library.get_item_name(itemId)
		var coefficient = mapGrid.meshTileTypeMap.get(itemName).get("coefficient")
		var gridCell2d = Vector2i(gridCell.x - offset_x, gridCell.z - offset_z)  # Adjust for offset
		astar_grid.set_point_solid(gridCell2d, false)
		astar_grid.set_point_weight_scale(gridCell2d, coefficient)
	return astar_grid

func find_path(startMap: Vector3i, goalMap: Vector3i) -> PackedVector2Array:
	var startMap2d = Vector2i(startMap.x - offset_x, startMap.z - offset_z)
	var goalMap2d = Vector2i(goalMap.x - offset_x, goalMap.z - offset_z)
	var path: PackedVector2Array = astar_grid.get_point_path(startMap2d, goalMap2d)

	# If you need the path in world coordinates, convert it back
	var world_path = PackedVector2Array()
	for point in path:
		world_path.append(Vector2(point.x + offset_x, point.y + offset_z))

	return world_path
