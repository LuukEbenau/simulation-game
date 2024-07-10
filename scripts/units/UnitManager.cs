using Godot;
using SacaSimulationGame.scripts.units;
using System;

public partial class UnitManager : Node3D
{
    [ExportCategory("Unit Scenes")]
    [Export]
    public PackedScene BuilderScene { get; set; }
    [Export]
    public PackedScene WorkerScene { get; set; }


    // Called when the node enters the scene tree for the first time.
    //private 
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public bool SpawnUnit(Vector3 spawnLocation, Unit unit) {
        Node3D instance = unit.Type switch
        {
            UnitType.BUILDER => this.BuilderScene.Instantiate<Node3D>(),
            UnitType.WORKER => this.WorkerScene.Instantiate<Node3D>(),
            _ => throw new Exception($"Unit type {unit.Type} is not handled by switch"),
        };

        instance.GlobalPosition = spawnLocation;

        AddChild(instance);

        return true;
    }
}
