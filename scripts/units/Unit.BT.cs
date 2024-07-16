﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using Godot;
using SacaSimulationGame.scripts.pathfinding;
using Windows.ApplicationModel.Background;

namespace SacaSimulationGame.scripts.units
{
    public partial class Unit : Node3D
    {
        /// <summary>
        /// Documentation: https://github.com/Eraclys/BehaviourTree/
        /// </summary>
        /// <returns></returns>
        protected abstract IBehaviour<UnitBTContext> GetBehaviourTree();

        public BehaviourStatus DoNothingSequence(UnitBTContext context)
        {
            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus FindPathToDestination(UnitBTContext context) {
            context.Path = null;
            context.CurrentPathIndex = 0;

            var startCell = MapManager.WorldToCell(GlobalPosition);
            var goalCell = MapManager.WorldToCell(context.Destination);
            var cellPath = MapManager.Pathfinder.FindPath(startCell, goalCell);

            if (cellPath.Count == 0) {

                return BehaviourStatus.Failed;
            }

            context.Path = cellPath
                .Select(node => new PathfindingNode3D(
                    MapManager.CellToWorld(node.Cell, height: MapManager.GetCell(node.Cell).Height + 0.2f, centered: true),
                    node.SpeedMultiplier)
                )
                .ToList();

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus MoveToDestination(UnitBTContext context)
        {
            if(context.Destination == default)
            {
                GD.Print("No destination defined");
                return BehaviourStatus.Failed;
            }

            PathfindingNode3D targetNode = context.Path[context.CurrentPathIndex];
            var direction = (targetNode.Position - GlobalPosition).Normalized();
            var movement = direction * speed * (float)context.Delta * targetNode.SpeedMultiplier;

            if (GlobalPosition.DistanceTo(targetNode.Position) > movement.Length())
            {
                GlobalTranslate(movement);
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
