using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.professions;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Stockpile : StorageBuildingBase
{
    [Export] public PackedScene ModelWood { get; set; }
    [Export] public PackedScene ModelEmpty { get; set; }
    [Export] public PackedScene ModelStone { get; set; }
    public override int MaxBuilders => 1;
   
    public override double TotalBuildingProgressNeeded => 2;

    public override BuildingType Type => BuildingType.Stockpile;

    private PackedScene _lastShownVisual;
    protected override void UpdateVisualBasedOnResources()
    {
        //TODO: only update if the visual has changed

        PackedScene visual;
        if (!BuildingCompleted)
        {
            // building in progress visual
            visual = ModelEmpty;
        }
        else
        {
            if (CurrentResourceStored == ResourceType.Wood)
            {
                visual = ModelWood;
            }
            else if(CurrentResourceStored == ResourceType.Stone)
            {
                visual = ModelStone;
            }
            else
            {
                visual = ModelEmpty; //TODO: modelstone
            }
        }
        //TODO: show percantage stored

        // only update if it changed
        if (visual != _lastShownVisual)
        {
            //TODO: different types of visuals?
            if (BuildingVisual != null) VisualWrap.RemoveChild(BuildingVisual);
            BuildingVisual = visual.Instantiate<Node3D>();

            VisualWrap.AddChild(BuildingVisual);
        }
        UpdateResourceAmountIndicator();
    }

    private int lastNrOfIndicators = -1;
    private void UpdateResourceAmountIndicator() {
        var itemPrefix = "item";

        var IndicatorChildren = BuildingVisual.GetChildren()
            .Where(c => c.Name.ToString().StartsWith(itemPrefix))
            .OrderBy(c => int.Parse(c.Name.ToString()[itemPrefix.Length..]))
            .Select(c => c as Node3D)
            .ToList();

        int nrOfIndicators = IndicatorChildren.Count;
        if (nrOfIndicators > 0)
        {
            var percentageFull = this.StoredResources.CurrentCapacity / this.StoredResources.MaxCapacity;

            var nrOfIndicatorsShown = Mathf.RoundToInt(percentageFull * nrOfIndicators);
            if (nrOfIndicatorsShown != lastNrOfIndicators)
            {
                for (int i = 0; i < nrOfIndicators; i++)
                {
                    var indicator = IndicatorChildren[i];
                    indicator.Visible = i < nrOfIndicatorsShown;
                }
                lastNrOfIndicators = nrOfIndicatorsShown;
            }
        }
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ResourcesRequiredForBuilding = new BuildingResources(0, 0);
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (this.BuildingCompleted)
        {
            //temporary: passive income
            var currentResource = StoredResources.TypesOfResourcesStored;

            if (currentResource == ResourceType.None) {
                var randNumber = new Random().Next(0, 2);
                if(randNumber == 0) currentResource = ResourceType.Stone;
                if (randNumber == 1) currentResource = ResourceType.Wood;
            }
            StoreResource(currentResource, (float)delta);
        }
    }
}
