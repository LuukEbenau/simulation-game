using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    public partial class Worker: Unit
    {

        private float speed = 5;
        public override void _Ready()
        {
            _rnd = new Random();
        }
        private Random _rnd;
        private bool arrived = false;
        private int currentPathIndex = 0;
        private List<Vector3> localPath;



        private void FollowPath(double delta)
        {
            Vector3 target = localPath[currentPathIndex];
            var direction = target - Position.Normalized();
            var movement = direction * speed * (float)delta;

            if (Position.DistanceTo(target) > movement.Length())
            {
                Position += movement;
            }
            else
            {
                //Position = target; // probably unneeded
                currentPathIndex++;

                if (currentPathIndex >= localPath.Count)
                {
                    arrived = true;
                    localPath = null;
                    GD.Print("Arrived at destination");
                    PerformAction();
                }
            }
        }

        private void PerformAction()
        {
            var cell = this.GameManager.MapManager.WorldToCell(GlobalPosition);
            //TOOD: actual functionality
            this.GameManager.BuildingManager.BuildBuilding(cell, new Road(), map.BuildingRotation.Bottom);
        }

        public override void _Process(double delta)
        {
            if (arrived || localPath == null)
            {
                var destinationCell = GetNewDestination();
                GD.Print($"Unit {this.Name} of type {this.UnitData.Type} moving to cell {destinationCell}");

                var currentCellPos = GameManager.MapManager.WorldToCell(GlobalPosition);
                List<Vector2I> cellPath = GameManager.MapManager.Pathfinder.FindPath(currentCellPos, destinationCell);

                localPath = cellPath.Select(cp => ToLocal(GameManager.MapManager.CellToWorldInterpolated(cp))).ToList();

                arrived = false;
                currentPathIndex = 0;
                GD.Print($"path is: {JsonConvert.SerializeObject(localPath)}");
            }
            else if (localPath != null && localPath.Count > 0)
            {
                FollowPath(delta);
            }
        }
    }
}
