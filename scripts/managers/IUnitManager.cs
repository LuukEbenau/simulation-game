using Godot;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.managers
{
    public interface IUnitManager
    {
        bool SpawnUnit(Vector3 spawnLocation, UnitDataObject unit);
    }
}