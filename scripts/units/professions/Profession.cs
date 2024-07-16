using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using Godot;
using SacaSimulationGame.scripts.pathfinding;
using Windows.Services.Maps;

namespace SacaSimulationGame.scripts.units.professions
{
    public abstract class Profession
    {
        public Unit Unit { get; }
        public IBehaviour<UnitBTContext> BehaviourTree { get; }

        /// <summary>
        /// 0 is novice, 1 is beginner, 2 is intermediate, 3 is educated, 4 is experienced, 5 is expert. Education brings all the way up to educated
        /// </summary>
        public int SkillLevel { get; } = 0;

        protected abstract IBehaviour<UnitBTContext> GetBehaviourTree();

        public Profession(Unit unit)
        {
            this.Unit = unit;
            this.BehaviourTree = GetBehaviourTree();
        }

        public BehaviourStatus DoNothingSequence(UnitBTContext context)
        {
            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus FindPathToDestination(UnitBTContext context)
        {
            context.Path = null;
            context.CurrentPathIndex = 0;

            var startCell = Unit.MapManager.WorldToCell(Unit.GlobalPosition);
            var goalCell = Unit.MapManager.WorldToCell(context.Destination);
            var cellPath = Unit.MapManager.Pathfinder.FindPath(startCell, goalCell);

            if (cellPath.Count == 0)
            {

                return BehaviourStatus.Failed;
            }

            context.Path = cellPath
                .Select(node => new PathfindingNode3D(
                    Unit.MapManager.CellToWorld(node.Cell, height: Unit.MapManager.GetCell(node.Cell).Height + 0.2f, centered: true),
                    node.SpeedMultiplier)
                )
                .ToList();

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus MoveToDestination(UnitBTContext context)
        {
            if (context.Destination == default)
            {
                GD.Print("No destination defined");
                return BehaviourStatus.Failed;
            }

            PathfindingNode3D targetNode = context.Path[context.CurrentPathIndex];
            var direction = (targetNode.Position - Unit.GlobalPosition).Normalized();
            var movement = direction * Unit.speed * (float)context.Delta * targetNode.SpeedMultiplier;

            if (Unit.GlobalPosition.DistanceTo(targetNode.Position) > movement.Length())
            {
                Unit.GlobalTranslate(movement);
            }
            else
            {
                context.CurrentPathIndex++;

                if (context.CurrentPathIndex >= context.Path.Count)
                {
                    GD.Print("Arrived at destination");
                    return BehaviourStatus.Succeeded;
                }
            }

            return BehaviourStatus.Running;
        }
    }
}
