class_name Unit
extends Node3D
#
#enum UnitType {BUILDER, SERVANT}
#
#var gameManager: GameManager
#
##Pathfinding
#var pathfinder:AstarPathfinder
#var arrived:bool = true
#var path = null
#var speed: float = 5.0
#var current_path_index: int= 0
#
#
#func setup(gameManager: GameManager):
    #print("setting up gamemanager %s" % gameManager)
    #self.gameManager = gameManager
#
func _on_ready():
    pass
    #if gameManager != null:
        #var grid = gameManager.gridmap
#
        #pathfinder = AstarPathfinder.new(grid)
        #pass
    #else:
        #print("gamemanager not set")
#
## Called every frame. 'delta' is the elapsed time since the previous frame.
#
#
func _process(delta):
    pass
    #if arrived or path == null:
        ## Get random house to travel to
        #var houseIndex = gameManager.gridmap.meshTileTypeMap.get("House").get("index")
        #var houses: Array[Vector3i] = gameManager.gridmap.get_used_cells_by_item(houseIndex)
        #print("Choice of %s houses" % houses.size())
        #var randomHouseMap: Vector3i = houses[randi() % houses.size()]
        ##var random_house_map = gameManager.gridmap.local_to_map(random_house)
        ##var random_house_2d = Vector2i(random_house_world.x, random_house_world.z)
#
        #var startPosMap = gameManager.gridmap.local_to_map(position)
#
        #print("Unit %s Moving to house %s" % [self.name, randomHouseMap])
        #print("Finding path from %s to %s" % [startPosMap, randomHouseMap])
#
        #var pathMap = pathfinder.find_path(startPosMap, randomHouseMap)
#
        #var pathLocal: Array[Vector3] = []
#
        #for mapCoord in pathMap:
            #var mapCoord3d = Vector3i(mapCoord.x,0,mapCoord.y)
            #var localCoord = gameManager.gridmap.map_to_local(mapCoord3d)
            ##var worldCoord = localCoord.to_global()
#
            #pathLocal.append(localCoord)
#
        #path = pathLocal
#
        #arrived = false
        #current_path_index = 0
        #print(path)
    #elif path.size() > 0:
        #var target = Vector3(path[current_path_index].x, path[current_path_index].y, path[current_path_index].z)
        #var direction = (target - position).normalized()
        #var movement = direction * speed * delta
#
        #if position.distance_to(target) > movement.length():
            #position += movement
        #else:
            #position = target # is this needed?
            #current_path_index += 1
#
            ## Check if we've completed the path
            #if current_path_index >= path.size():
                #arrived = true
                #path = null
                #print("Arrived at destination")
                ## You might want to emit a signal or call a method here to handle arrival
            ##else:
                ### Optionally, you could update the direction to the next point here
                ### This can make movement around corners look smoother
                ##var next_target = path[current_path_index]
                ##direction = (next_target - position).normalized()
