using System.Collections.Generic;
using Godot;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.managers;

namespace SacaSimulationGame.scripts.building
{
    public interface IBuildingManager
    {
        BuildingManager.BuildingTypeIdPair[,] OccupiedCells { get; }

        bool BuildBuilding(Vector2I cell, BuildingBlueprintBase buildingBlueprint, bool isBase, bool isDestination, float baseHeight);
        BuildingDataObject GetBuildingAtCell(Vector2I cell);
        List<BuildingDataObject> GetBuildings();
    }
}