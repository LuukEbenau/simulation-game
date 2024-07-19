using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.professions;
using System;

public partial class Stockpile : StorageBuildingBase
{
    [Export]
    public PackedScene ModelWood { get; set; }

    public override int MaxBuilders => 1;
   
    public override double TotalBuildingProgressNeeded => 2;

    public override BuildingType Type => BuildingType.Stockpile;
    


    protected override void UpdateVisualBasedOnResources()
    {
        if (BuildingVisual != null) RemoveChild(BuildingVisual);

        //TODO: different types of visuals?
        BuildingVisual = this.ModelWood.Instantiate<Node3D>();

        AddChild(BuildingVisual);
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
