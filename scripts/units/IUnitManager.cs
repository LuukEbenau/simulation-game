using Godot;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    public interface IUnitManager
    {
        bool SpawnUnit(Vector3 spawnLocation, UnitDataObject unit);
    }
}