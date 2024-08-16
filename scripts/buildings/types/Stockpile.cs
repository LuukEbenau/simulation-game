using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.professions;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Stockpile : StorageBuildingBase
{
    [Export] public PackedScene ModelWood { get; set; }

    [Export] public PackedScene ModelStone { get; set; }
    public override int MaxBuilders => 1;
   
    public override float TotalBuildingProgressNeeded => 2;

    public override BuildingType Type => BuildingType.Stockpile;

    private PackedScene _lastShownVisual;

    public override void _Ready()
    {
        base._Ready();
    }


    private ResourceType _randomlyDecidedResource { get; set; }
    public override void _Process(double delta)
    {
        base._Process(delta);

        if (this.BuildingCompleted)
        {
            
            //temporary: passive income
            var currentResource = StoredResources.TypesOfResourcesStored;

            if (currentResource == 0)
            {
                if(_randomlyDecidedResource == 0)
                {
                    var randNumber = new Random().Next(0, 2);
                    if (randNumber == 0) currentResource = ResourceType.Stone;
                    if (randNumber == 1) currentResource = ResourceType.Wood;
                    _randomlyDecidedResource = currentResource;
                }
                else
                {
                    currentResource = _randomlyDecidedResource;
                }
            }
            StoredResources.AddResource(currentResource, (float)delta);
        }
    }

    protected override void UpdateVisualBasedOnResources()
    {
        PackedScene visual;
        if (!BuildingCompleted)
        {
            visual = ModelConstruction;
        }
        else
        {
            if (StoredResources.TypesOfResourcesStored == ResourceType.Wood)
            {
                visual = ModelWood;
            }
            else if(StoredResources.TypesOfResourcesStored == ResourceType.Stone)
            {
                visual = ModelStone;
            }
            else
            {
                visual = ModelCompleted;
            }
        }

        // only update if it changed
        if (visual != _lastShownVisual)
        {
            if (BuildingVisual != null) {
                foreach (var child in VisualWrap.GetChildren())
                {
                    VisualWrap.RemoveChild(child);
                    //child.QueueFree();
                }
            } 

            BuildingVisual = visual.Instantiate<Node3D>();

            VisualWrap.AddChild(BuildingVisual);
            _lastShownVisual = visual;
        }

        UpdateResourceAmountIndicator();
    }

    private int lastNrOfIndicators = -1;
    List<Node3D> indicators = new();
    private Node3D lastIndicator;
    private void UpdateResourceAmountIndicator() {
        var itemPrefix = "item";

        // only load indicators if resource type changes
        if(StoredResources.TypesOfResourcesStored == 0){
            return;
        }
        if(BuildingVisual != lastIndicator){
            lastIndicator = BuildingVisual;
            indicators = BuildingVisual.GetChildren()
                .Where(c => c.Name.ToString().StartsWith(itemPrefix))
                .OrderBy(c => c.Name.ToString())//int.Parse(c.Name.ToString()[itemPrefix.Length..]))
                .Select(c => c as Node3D)
                .ToList();
        }

        int nrOfIndicators = indicators.Count;
        if (nrOfIndicators > 0)
        {
            var currentStoredResources = StoredResources.GetResourcesOfType(StoredResources.TypesOfResourcesStored);
            var maxStoredResources = StoredResources.GetStorageCapacityLeft(StoredResources.TypesOfResourcesStored);

            
            var percentageFull = (currentStoredResources <= 0) ? 0 : currentStoredResources / (maxStoredResources + currentStoredResources);

            var nrOfIndicatorsShown = Mathf.RoundToInt(percentageFull * nrOfIndicators);
            if (nrOfIndicatorsShown != lastNrOfIndicators)
            {
                for (int i = 0; i < nrOfIndicators; i++)
                {
                    var indicator = indicators[i];
                    indicator.Visible = i < nrOfIndicatorsShown;
                }
                lastNrOfIndicators = nrOfIndicatorsShown;
            }
        }
    }


}
