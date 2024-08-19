using Godot;
using SacaSimulationGame.scripts.buildings;
using System;

public partial class Fishingpost : StorageBuildingBase
{
    public override int MaxBuilders => 3;
    public override float TotalBuildingProgressNeeded => 30;
    public override bool IsResourceStorage => false;
    public override StorageStrategyEnum StorageStrategy => StorageStrategyEnum.EmptyAllResources;
    public override BuildingType Type => BuildingType.FishingPost;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    protected override void UpdateVisualBasedOnResources()
    {
        GD.Print($"TODO: update visual of fishing post");
        //throw new NotImplementedException();
    }
}
