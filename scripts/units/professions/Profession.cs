using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.helpers;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.pathfinding;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.units.tasks;

namespace SacaSimulationGame.scripts.units.professions
{
    public abstract class Profession
    {

        private float GetSkillSpeedMultiplier()
        {
            return MathF.Sqrt(SkillLevel + 1);
        }

        protected float GetActivitySpeedCoefficient()
        {
            return GetSkillSpeedMultiplier() * ActivitySpeedBaseline;
        }

        protected abstract BuildingType ProfessionBuildingType { get; }
        protected abstract float ActivitySpeedBaseline { get; }

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
                        //.UntilSuccess("Find valid destination")
                        .Do("Get random position to move to", this.GetRandomNearbyLocation)
                        .Do("Find Path to nearby position", this.FindPathToDestination)
                        //.End()
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
            for (int retryCount = 0; retryCount < 10; retryCount++)
            {
                var dir = new Vector3(rand.Next(-5, 5), 0, rand.Next(-5, 5));

                var newPos = Unit.GlobalPosition + dir;

                var newCell = Unit.MapManager.WorldToCell(newPos);

                if(Unit.MapManager.CellIsTraversable(newCell, map.CellType.GROUND))
                {
                    context.Destination = newPos;
                    return BehaviourStatus.Succeeded;
                }
            }

            return BehaviourStatus.Failed;
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
                    Unit.MapManager.CellToWorld(node.Cell, height: Unit.MapManager.GetCell(node.Cell).Height + 0.15f, centered:true),
                    node.SpeedMultiplier)
                )
                .ToList();

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus FollowPath(UnitBTContext context)
        {
            if (context.Path == default || context.Path.Count == 0)
            {
                GD.Print($"'{Unit.UnitName}': No destination defined, tree status name: {this.BehaviourTree.Name}");
                return BehaviourStatus.Failed;
            }

            PathfindingNode3D targetNode;
            try
            {
                targetNode = context.Path[context.CurrentPathIndex];
            }
            catch(Exception ex)
            {
                throw new Exception($"'{Unit.UnitName}': An error has occured getting the next cell in the path: path index {context.CurrentPathIndex}, with path of length {context.Path.Count} | tree status: {this.BehaviourTree.Name}", ex);
            }
            var direction = (targetNode.Position - Unit.GlobalPosition).Normalized();

            float groundSurfaceSpeedMultiplier = targetNode.SpeedMultiplier;
            if (context.CurrentPathIndex > 0) groundSurfaceSpeedMultiplier = Mathf.Max(groundSurfaceSpeedMultiplier, context.Path[context.CurrentPathIndex - 1].SpeedMultiplier);
            
            var movement = direction * Unit.Stats.Speed * (float)context.Delta * groundSurfaceSpeedMultiplier;

            if (Unit.GlobalPosition.DistanceTo(targetNode.Position) > movement.Length() )
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

        #region resource gathering
        protected BehaviourStatus ChopTree(UnitBTContext context)
        {

            var resourcesToTake = (float)context.Delta * GetActivitySpeedCoefficient();


            var t = context.AssignedTask as NaturalResourceGatherTask;

            var (resourcesTaken, resourceType) = t.Resource.CollectResource(resourcesToTake);

            if (resourcesTaken == 0 || resourceType == default)
            {
                //resource has disappeared while we were moving to its location
                //TODO: should we detect it disappearing already while it is walking?
                GD.Print($"'{Unit.UnitName}': no resource could be found");
                return BehaviourStatus.Succeeded; // finished chopping
            }

            Unit.Inventory.AddResource(resourceType, resourcesTaken);

            context.WaitingTime += context.Delta;
            var timeoutTime = 30;

            //failsafe:
            if (context.WaitingTime > timeoutTime)
            {
                return BehaviourStatus.Failed;
            }


            if (t.Resource.GetNrOfResourcesLeft() == 0)
            {
                return BehaviourStatus.Succeeded;
            }
            if (Unit.Inventory.GetStorageCapacityLeft(resourceType) == 0)
            {
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Running;
        }
        #endregion

        #region profession building
        protected IBehaviour<UnitBTContext> MoveToProfessionBuildingAndDropoffResources => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Move to Profession Building And Drop off Resources")
                .Do("Set destination", SetDestinationToProfessionBuilding)
                .Do("Find path", FindPathToDestination)
                .Do("Follow path", FollowPath)
                .Do("Drop off Resources", DropOfResourcesAtProfessionBuilding)
            .End()
        .Build();


        protected BehaviourStatus FindProfessionBuilding(UnitBTContext context)
        {
            var closestBuilding = Unit.BuildingManager.GetBuildings()
                .Where(b => this.ProfessionBuildingType.HasFlag(b.Instance.Type) && b.Instance.WorkingEmployees.Count < b.Instance.MaxNumberOfEmployees)
                .OrderBy(b => b.Instance.WorkingEmployees.Count)
                .ThenBy(b => Unit.GlobalPosition.DistanceTo(b.Instance.GlobalPosition))
                .FirstOrDefault();

            if (closestBuilding == null) return BehaviourStatus.Failed;

            ProfessionBuilding = closestBuilding.Instance as StorageBuildingBase;
            ProfessionBuilding.AddEmployee(Unit);

            return BehaviourStatus.Succeeded;
        }
        protected bool HasProfessionBuilding(UnitBTContext context)
        {
            if (this.ProfessionBuilding == null) return false;
            //TODO: what if a building is destroyed? this has to be taken into account
            return true;
        }

        protected BehaviourStatus SetDestinationToProfessionBuilding(UnitBTContext context)
        {
            context.Destination = ProfessionBuilding.GlobalPosition;

            return BehaviourStatus.Succeeded;
        }


        protected BehaviourStatus DropOfResourcesAtProfessionBuilding(UnitBTContext context)
        {
            foreach (var resourceType in Unit.Inventory.TypesOfResourcesStored.GetActiveFlags())
            {
                var storageSpace = ProfessionBuilding.StoredResources.GetStorageCapacityLeft(resourceType);

                var amountRemoved = Unit.Inventory.RemoveResource(resourceType, storageSpace);
                
                if (amountRemoved > 0)
                {
                    var leftOver = ProfessionBuilding.StoredResources.AddResource(resourceType, amountRemoved);
                    if (leftOver > 0)
                    {
                        GD.PushWarning($"'{Unit.UnitName}': Resource leftover of {leftOver} of resource {resourceType}, this should never happen here.");
                        Unit.Inventory.AddResource(resourceType, leftOver);
                    }
                }
                GD.Print($"'{Unit.UnitName}': Dropped of {amountRemoved} resources at profession building, with storage space {storageSpace}");
            }

            return BehaviourStatus.Succeeded;
        }

        #endregion
    }
}
