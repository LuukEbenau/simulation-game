using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.building;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.storages;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions;
using SacaSimulationGame.scripts.units.professions.misc;

public partial class Unit : Node3D
{
    [ExportCategory("Model References")]
    [Export]
    public PackedScene BuilderModel { get; set; }
    [Export]
    public PackedScene WorkerModel { get; set; }
    [Export] PackedScene LumberjackModel { get; set; }

    public Profession Profession { get; private set; }
    public UnitStats Stats { get; private set; } = new UnitStats { Speed = 5 };
    public StorageBase Inventory { get; private set; }


    private static int _unitIdxCounter = 0;
    private int UnitIdx = 0;
    public string UnitName { get; set; }

    public GameManager GameManager { get; set; }
    public IBuildingManager BuildingManager => GameManager.BuildingManager;
    public IUnitManager UnitManager => GameManager.UnitManager;
    public IWorldMapManager MapManager => GameManager.MapManager;

    protected UnitBTContext context;

    private Node3D VisualModel { get; set; } = null;

    public Unit()
    {
        UnitIdx = _unitIdxCounter++;
        UnitName = $"Unit {UnitIdx}"; // failsafe in case no name is selected
    }

    public void ChangeProfession(ProfessionType professionType)
    {
        if (VisualModel != null) RemoveChild(VisualModel);
        VisualModel = null;

        if (professionType == ProfessionType.Worker)
        {
            this.Profession = new WorkerProfession(this);
            VisualModel = WorkerModel.Instantiate<Node3D>();
            UnitName = $"Worker {UnitIdx}";
        }
        else if (professionType == ProfessionType.Builder)
        {
            this.Profession = new BuilderProfession(this);
            VisualModel = BuilderModel.Instantiate<Node3D>();
            UnitName = $"Builder {UnitIdx}";
        }
        else if (professionType == ProfessionType.Lumberjack)
        {
            this.Profession = new LumberjackProfession(this);
            this.VisualModel = LumberjackModel.Instantiate<Node3D>();
            UnitName = $"Lumberjack {UnitIdx}";
        }
        else
        {
            throw new Exception($"unknown profession type {professionType}");
        }
        //update visuals based on profession

        AddChild(VisualModel);
    }

    private Node3D ResourceIndicator { get; set; }
    public override void _Ready()
    {
        base._Ready();
        this.Inventory = GetNode<GeneralStorage>("Inventory");
        this.ResourceIndicator = GetNode<Node3D>("ResourceIndicator");

        this.Inventory.StoredResourcesChanged += Inventory_StoredResourcesChanged;
    }

    private void Inventory_StoredResourcesChanged()
    {
        var woodIndicator = ResourceIndicator.GetNode<Node3D>("Wood");
        var stoneIndicator = ResourceIndicator.GetNode<Node3D>("Stone");

        woodIndicator.Visible = this.Inventory.TypesOfResourcesStored.HasFlag(ResourceType.Wood);
        stoneIndicator.Visible = this.Inventory.TypesOfResourcesStored.HasFlag(ResourceType.Stone);
    }

    public override void _Process(double delta)
    {
        context ??= new UnitBTContext();
        context.Delta = delta;

        try
        {
            var status = this.Profession.BehaviourTree.Tick(context);
            if (status == BehaviourStatus.Failed)
            {
                context = new UnitBTContext();
                //this.Profession.BehaviourTree.Reset();
            }
        }
        catch (Exception ex) {
            context = new UnitBTContext();
            this.Profession.BehaviourTree.Reset();
            GD.PushWarning($"Error occured while running Behaviour tree",ex);

        }

    }
}
