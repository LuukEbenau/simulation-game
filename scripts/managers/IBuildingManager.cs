using System.Collections.Generic;
using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataObjects;

namespace SacaSimulationGame.scripts.managers
{
    public interface IBuildingManager
    {
        BuildingManager.BuildingTypeIdPair[,] OccupiedCells { get; }

        bool BuildBuilding(Vector2I cell, BuildingBlueprintBase buildingBlueprint);
        BuildingDataObject GetBuildingAtCell(Vector2I cell);
        List<BuildingDataObject> GetBuildings();
    }
}