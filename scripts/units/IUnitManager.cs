using System.Collections.Generic;
using Godot;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.tasks;

namespace SacaSimulationGame.scripts.units
{
    public interface IUnitManager
    {
        bool SpawnUnit(Vector3 spawnLocation, UnitDataObject unit);
        //IEnumerable<UnitTask> GetUnitTasks();
        //void EnqueueTask(UnitTask task);
        //bool FinishTask(UnitTask task);
    }
}