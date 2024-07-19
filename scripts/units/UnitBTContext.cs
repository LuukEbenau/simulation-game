using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.pathfinding;

namespace SacaSimulationGame.scripts.units
{
    public class UnitBTContext
    {
        public double Delta { get; set; }
        public Vector3 Destination {  get; set; }
        public BuildingDataObject Building { get; set; }

        public int CurrentPathIndex {  get; set; }
        public List<PathfindingNode3D> Path { get; set; }

        public double WaitingTime { get; set; } = 0;

        public StorageBuildingBase ResourcePickupBuilding { get; set; }
    }
}
