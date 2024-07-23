using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.pathfinding;
using SacaSimulationGame.scripts.units.professions.misc;
using Windows.Services.Maps;

namespace SacaSimulationGame.scripts.units.professions
{
    public abstract class Profession
    {
        public Unit Unit { get; }

        /// <summary>
        /// The building assigned to the unit based on it's profession
        /// </summary>
        public StorageBuildingBase ProfessionBuilding { get; set; }

        public IBehaviour<UnitBTContext> BehaviourTree { get; }

        private float idleBehaviourDuration = 2;

        /// <summary>
        /// 0 is novice, 1 is beginner, 2 is intermediate, 3 is educated, 4 is experienced, 5 is expert. Education brings all the way up to educated
        /// </summary>
        public int SkillLevel { get; } = 0;

        protected abstract IBehaviour<UnitBTContext> GetBehaviourTree();

        private IBehaviour<UnitBTContext> _idleBehaviourTree;

        public IBehaviour<UnitBTContext> IdleBehaviourTree { 
            get {
                _idleBehaviourTree ??= FluentBuilder.Create<UnitBTContext>()
                    .Sequence("Idle Sequence")
                        .Do("Get random position to move to", this.GetRandomNearbyLocation)
                        .Do("Find Path to nearby position", this.FindPathToDestination)
                        .Do("follow path", this.FollowPath)
                        .Do("Idle", this.DoNothingSequence)
                    .End()
                .Build();

                return _idleBehaviourTree;
            }
        } 

        public BehaviourStatus GetRandomNearbyLocation(UnitBTContext context)
        {
            var rand = new Random();

            var dir = new Vector3(rand.Next(-5, 5), 0, rand.Next(-5, 5));

            var newPos = Unit.GlobalPosition + dir;

            context.Destination = newPos;
            return BehaviourStatus.Succeeded;
        }

        public Profession(Unit unit)
        {
            this.Unit = unit;
            this.BehaviourTree = GetBehaviourTree();
        }

        /// <summary>
        /// do nothing for a few seconds
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public BehaviourStatus DoNothingSequence(UnitBTContext context)
        {
            context.WaitingTime += context.Delta;

            

            if(context.WaitingTime > idleBehaviourDuration)
            {
                context.WaitingTime = 0;
                return BehaviourStatus.Succeeded;
            }
            return BehaviourStatus.Running;
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
                    Unit.MapManager.CellToWorld(node.Cell, height: Unit.MapManager.GetCell(node.Cell).Height + 0.2f, centered:true),
                    node.SpeedMultiplier)
                )
                .ToList();

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus FollowPath(UnitBTContext context)
        {
            if (context.Path == default || context.Path.Count == 0)
            {
                GD.Print("No destination defined");
                return BehaviourStatus.Failed;
            }

            PathfindingNode3D targetNode = context.Path[context.CurrentPathIndex];
            var direction = (targetNode.Position - Unit.GlobalPosition).Normalized();
            var movement = direction * Unit.Stats.Speed * (float)context.Delta * targetNode.SpeedMultiplier;

            if (Unit.GlobalPosition.DistanceTo(targetNode.Position) > movement.Length())
            {
                Unit.GlobalTranslate(movement);
            }
            else
            {
                context.CurrentPathIndex++;

                if (context.CurrentPathIndex >= context.Path.Count)
                {
                    return BehaviourStatus.Succeeded;
                }
            }

            return BehaviourStatus.Running;
        }

        private BehaviourStatus IsInBuildingDistance(UnitBTContext context)
        {
            var buildingPos = Unit.GameManager.MapManager.CellToWorld(context.Building.Instance.Cell, centered: true);

            if (Unit.GlobalPosition.DistanceTo(buildingPos) <= 1.0)
            {
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Failed;
        }
    }
}
