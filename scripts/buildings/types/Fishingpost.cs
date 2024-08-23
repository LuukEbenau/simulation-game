using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.units.tasks;
using System;
using System.Linq;

public partial class Fishingpost : StorageBuildingBase
{
    public override int MaxBuilders => 3;
    public override float TotalBuildingProgressNeeded => 30;
    public override StorageStrategyEnum StorageStrategy => StorageStrategyEnum.EmptyAllResources;
    public override BuildingType Type => BuildingType.FishingPost;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();
        StoredResources.StoredResourcesChanged += StoredResources_StoredResourcesChanged;
    }

    private void StoredResources_StoredResourcesChanged()
    {
        if (StoredResources.CurrentCapacity > 0)
        {
            var currentTask = GameManager.TaskManager.GetTasks().Where(t => t is PickupResourcesTask pt && pt.Building == this).FirstOrDefault();
            if (currentTask == null)
            {
                var task = new PickupResourcesTask(this);
                GameManager.TaskManager.EnqueueTask(task);
            }
        }
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
