using Godot;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;
using System;

public partial class UnitManager : Node3D
{
    [ExportCategory("Unit Scenes")]
    [Export]
    public PackedScene BuilderScene { get; set; }
    [Export]
    public PackedScene WorkerScene { get; set; }


    private GameManager GameManager { get; set; }
    // Called when the node enters the scene tree for the first time.
    //private 
    public override void _Ready()
    {
        this.GameManager = this.GetParent<GameManager>();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public bool SpawnUnit(Vector3 spawnLocation, UnitDataObject unit) {
        Unit instance;
        if (unit.Type == UnitType.WORKER)
        {
            instance = this.WorkerScene.Instantiate<Worker>();
        }
        else if (unit.Type == UnitType.BUILDER) {
            instance = this.BuilderScene.Instantiate<Builder>();
        }
        else
        {
            throw new Exception("undefined instance type");
        }

        instance.UnitData = unit;
        instance.GameManager = this.GameManager;
        instance.GlobalPosition = spawnLocation;

        AddChild(instance);

        return true;
    }
}
