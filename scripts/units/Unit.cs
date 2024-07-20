using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions;

public partial class Unit : Node3D
{
    [ExportCategory("Model References")]
    [Export]
    public PackedScene BuilderModel { get; set; }
    [Export]
    public PackedScene WorkerModel { get; set; }

    public Profession Profession { get; private set; }
    public UnitStats Stats { get; private set; } = new UnitStats { Speed = 5 };
    public UnitInventory Inventory { get; private set; } = new UnitInventory(50);


    public GameManager GameManager { get; set; }
    public BuildingManager BuildingManager => GameManager.BuildingManager;
    public UnitManager UnitManager => GameManager.UnitManager;
    public WorldMapManager MapManager => GameManager.MapManager;


    //public UnitDataObject UnitData { get; set; }

    //public readonly float speed = 5;

    protected UnitBTContext context;

    private Node3D VisualModel { get; set; } = null;

    public void ChangeProfession(ProfessionType professionType)
    {
        if (VisualModel != null) RemoveChild(VisualModel);
        VisualModel = null;

        if (professionType == ProfessionType.Worker)
        {
            this.Profession = new WorkerProfession(this);
            VisualModel = WorkerModel.Instantiate<Node3D>();
        }
        else if (professionType == ProfessionType.Builder)
        {
            this.Profession = new BuilderProfession(this);
            VisualModel = BuilderModel.Instantiate<Node3D>();
        }
        else
        {
            throw new Exception($"unknown profession type {professionType}");
        }
        //update visuals based on profession

        AddChild(VisualModel);
    }
    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        context ??= new UnitBTContext();
        context.Delta = delta;

        var status = this.Profession.BehaviourTree.Tick(context);
        if (status == BehaviourStatus.Failed)
        {
            this.Profession.BehaviourTree.Reset();
        }

    }
}
