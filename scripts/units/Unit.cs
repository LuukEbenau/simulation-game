using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    public abstract partial class Unit: Node3D
    {
        private Random _rnd;
        public Unit()
        {
            _rnd = new Random();
        }
        public GameManager GameManager { get; set; }
        public UnitDataObject UnitData { get; set; }
        private List<Vector3> path;
        private readonly float speed = 5;

        private bool arrived = false;
        private int currentPathIndex = 0;

        protected abstract void PerformAction();
        protected Vector2I GetNewDestination()
        {
            Vector3 destination = new(_rnd.Next(-50, 50), GlobalPosition.Y, _rnd.Next(-50, 50));

            var destinationCell = GameManager.MapManager.WorldToCell(GlobalPosition + destination);
            return destinationCell;
        }

        protected void ManagePathfinding(double delta)
        {
            if (arrived || path == null)
            {
                var destinationCell = GetNewDestination();
                var currentCellPos = GameManager.MapManager.WorldToCell(GlobalPosition);

                GD.Print($"Unit {this.Name} of type {this.UnitData.Type} moving from {currentCellPos} to {destinationCell}");

                List<Vector2I> cellPath = GameManager.MapManager.Pathfinder.FindPath(currentCellPos, destinationCell);

                path = cellPath.Select(cp => GameManager.MapManager.CellToWorldInterpolated(cp, height: GlobalPosition.Y)).ToList();

                arrived = false;
                currentPathIndex = 0;
                //GD.Print($"path is: {JsonConvert.SerializeObject(path)}");
            }
            else if (path != null && path.Count > 0)
            {
                FollowPath(delta);
            }
            else if (path.Count == 0) {
                //GD.Print("Path is not reachable");
                path = null;
            }
        }

        protected void FollowPath(double delta)
        {
            Vector3 target = path[currentPathIndex];
            var direction = (target - GlobalPosition).Normalized();
            var movement = direction * speed * (float)delta;

            if (GlobalPosition.DistanceTo(target) > movement.Length())
            {
                GlobalTranslate(movement);
            }
            else
            {
                //GlobalPosition = target; // probably unneeded
                currentPathIndex++;

                if (currentPathIndex >= path.Count)
                {
                    arrived = true;
                    path = null;
                    GD.Print("Arrived at destination");
                    PerformAction();
                }
            }
        }
    }
}
