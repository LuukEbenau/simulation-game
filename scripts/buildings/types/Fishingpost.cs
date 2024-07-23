using Godot;
using SacaSimulationGame.scripts.buildings;
using System;

public partial class Fishingpost : BuildingBase
{
    public override int MaxBuilders => 3;
    public override double TotalBuildingProgressNeeded => 30;
    public override bool IsResourceStorage => false;
    public override BuildingType Type => BuildingType.FishingPost;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        // new BuildingResources(50, 10);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
