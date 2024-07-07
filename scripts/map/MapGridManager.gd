class_name MapGridManager
extends Node3D

#region initialization
@export var terrain: Node3D
@export var show_slope_gradients: bool = false:
    set(value):
        show_slope_gradients = value
        if gradient_visualizer:
            gradient_visualizer.show_slope_gradients = value

@onready var gradient_visualizer = $TerrainGradientVisualizer

var cell_size = Vector2i(1,1)


var terrainGradients:Dictionary

#endregion initialization

#region node events
func _ready():
    var terrainMapper = $TerrainMapper
    var grid: Dictionary = terrainMapper.map_terrain(terrain, cell_size)
    terrainMapper.print_terrain_info(grid, cell_size)

    var loadResult = terrainMapper.load_terrain_gradients()
    if loadResult[0]:
        terrainGradients = loadResult[1]
        print("Loaded terrain gradients from file")
    else:
        print("Calculating terrain gradients...")
        terrainGradients = terrainMapper.calculate_terrain_gradients(grid,cell_size)
        terrainMapper.save_terrain_gradients(terrainGradients)
        print("Terrain gradients calculated and saved")

    gradient_visualizer.position = Vector3(terrain.global_transform.origin.x,gradient_visualizer.position.y, terrain.global_transform.origin.z)
    gradient_visualizer.set_gradients(terrainGradients, Vector2(cell_size.x, cell_size.y))
    gradient_visualizer.show_slope_gradients = show_slope_gradients

#endregion
