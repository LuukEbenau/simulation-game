class_name GameManager
extends Node3D

@export var gridmap:GridMapMain
#var builder_scene = preload("res://assets/units/unit.tscn")

var spawn_location = Vector3(3,0,3)

# Called when the node enters the scene tree for the first time.
#func _ready():
    #spawn_unit(self.spawn_location)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
    pass


func spawn_unit(spawn_location:Vector3):
    var builder_instance = UnitFactory.new_unit(UnitFactory.UnitType.BUILDER, self)
    builder_instance.position = spawn_location
    add_child(builder_instance)

    var servant_instance = UnitFactory.new_unit(UnitFactory.UnitType.SERVANT, self)
    servant_instance.position = spawn_location + Vector3(0,0,2)
    add_child(servant_instance)

