using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.pathfinding;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.naturalResources.instances;
using SacaSimulationGame.scripts.units.tasks;

namespace SacaSimulationGame.scripts.units.professions.misc
{
    public class UnitBTContext
    {
        /// <summary>
        /// Delta Time
        /// </summary>
        public double Delta { get; set; }

        public UnitTask AssignedTask { get; set; }
        public BuildingDataObject Building { get; set; }
        public StorageBuildingBase ResourceStorageBuilding { get; set; }

        public Vector3 Destination { get; set; }
        public int CurrentPathIndex { get; set; }
        public List<PathfindingNode3D> Path { get; set; }

        /// <summary>
        /// When the unit has to idle, this is the counter of how long it has been idling
        /// </summary>
        public double WaitingTime { get; set; } = 0;
    }
}
