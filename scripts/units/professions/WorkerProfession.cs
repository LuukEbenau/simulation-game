﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree.FluentBuilder;
using BehaviourTree;
using Godot;
using SacaSimulationGame.scripts.buildings;
using System.Net.Mime;
using SacaSimulationGame.scripts.pathfinding;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.helpers;
using SacaSimulationGame.scripts.units.tasks;
using System.Diagnostics;

namespace SacaSimulationGame.scripts.units.professions
{
    public class WorkerProfession(Unit unit) : Profession(unit)
    {
        protected override BuildingType ProfessionBuildingType => BuildingType.None;

        private readonly Random _rnd = new();
        protected override float ActivitySpeedBaseline => 1;

        //TODO: behaviour for emptying the stored resources of resource gathering buildings
        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            //1. find target
            //if has resources: drop of resources
            //else: pick up resources
            return FluentBuilder.Create<UnitBTContext>()
                .Selector("Behaviour selector")
                    .Sequence("")
                        .Condition("Check if unit carries resources", UnitCarriesResources)
                        //IF task is found with the workers resource
                        .Subtree(DropOfResourcesSubtree)
                    .End()
                    
                    .Subtree(TaskExecutionSubtree)
                    //.Subtree(DeliverResourcesToBuildingSubtree)
                    .Subtree(IdleBehaviourTree)
                .End()
                .Build();
        }
        #region executing tasks

        private float _getWeightedBuildingToBuildValue(Unit unit, BuildBuildingTask task)
        {
            const float assignedUnitCoefficient = 3;
            const float prioritizeBuildingsWhichWaitForResourcesCoefficient = 2;
            var nrOfAssignedUnitsValue = task.Building.GetNrOfAssignedUnits * assignedUnitCoefficient;

            // between 0 and 1
            float percentDiff = task.Building.Instance.BuildingResources.PercentageResourcesAquired - task.Building.Instance.BuildingPercentageComplete; // big is bad
            float amountOfResourcesStillRequiredValue = prioritizeBuildingsWhichWaitForResourcesCoefficient - (percentDiff * prioritizeBuildingsWhichWaitForResourcesCoefficient);

            float distance = unit.GlobalPosition.DistanceTo(task.TaskPosition);

            float value = (nrOfAssignedUnitsValue + amountOfResourcesStillRequiredValue) / distance;

            return 1f / value;
        }

        private IBehaviour<UnitBTContext> TaskExecutionSubtree => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Perform task")
                
                .RandomSelector("Task selector")
                    .Sequence("TASK VALIDATION: Deliver Resources To building")
                        .Do("FindTask", c => FindTask<BuildBuildingTask>(c, 
                            (t) => {
                                return t.Building.Instance.BuildingResources.PercentageResourcesAquired < 1;
                            },
                            (items) => {
                                return items.OrderBy(t => _getWeightedBuildingToBuildValue(Unit, t));
                            }
                        ))
                        .Subtree(DeliverResourcesToBuildingSubtree) 
                    .End()
                    .Sequence("TASK VALIDATION: Natural Resource collection")
                        .Do("FindTask", c => FindTask<NaturalResourceGatherTask>(c, 
                            (t) => !t.IsFinished,
                            null
                            )
                        )
                        .Subtree(NaturalResourceGatherSubtree)
                    .End()

                // Any other types of tasks
                .End()
            .End()
        .Build();

        public BehaviourStatus FindTask<T>(UnitBTContext context, Func<T, bool> condition, Func<IEnumerable<T>, IOrderedEnumerable<T>> orderCriteria) where T : UnitTask
        {
            var type = typeof(T);
            var availableTasks = this.Unit.GameManager.TaskManager.GetTasks()
                .Where(ut => type.Equals(ut.GetType()))
                .Select(t=> (T)t)
                .Where(t=> condition(t))
                .ToList();

            if (availableTasks.Count == 0)
            {
                return BehaviourStatus.Failed;
            }

            IOrderedEnumerable<T> availableTasksOrdered;

            if(orderCriteria == null)
            {
                availableTasksOrdered = availableTasks
                    .OrderBy(ut => ut.TaskPosition.DistanceTo(Unit.GlobalPosition));
            }
            else
            {
                availableTasksOrdered = orderCriteria(availableTasks)
                    .ThenBy(ut => ut.TaskPosition.DistanceTo(Unit.GlobalPosition));
            }

            var availableTask = availableTasks.First();

            if (availableTask.IsFinished) //NOTE: not sure if i should do this here, but might be good safety check to not get stuck on a orphan task
            {
                this.Unit.GameManager.TaskManager.FinishTask(availableTask);
                return BehaviourStatus.Failed;
            }

            context.AssignedTask = availableTask;
            context.Destination = availableTask.TaskPosition;

            return BehaviourStatus.Succeeded;
        }

        private BehaviourStatus SetDestinationToTaskLocation(UnitBTContext context)
        {
            //var cell = Unit.MapManager.WorldToCell(context.AssignedTask.TaskPosition);

            context.Destination = context.AssignedTask.TaskPosition;

            return BehaviourStatus.Succeeded;
        }

        #region TASK: natural resource gathering
        private IBehaviour<UnitBTContext> NaturalResourceGatherSubtree => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Gather natural resources")
                .Do("Set destination", SetDestinationToTaskLocation)
                .Do("Move to destination", this.FindPathToDestination)
                .Do("Navigate", this.FollowPath)
                .Do("Gather resource", GatherNaturalResource)
            .End()
        .Build();

        private BehaviourStatus GatherNaturalResource(UnitBTContext context)
        {
            var resourceGatherTask = context.AssignedTask as NaturalResourceGatherTask;
            if (resourceGatherTask.Resource is TreeResource treeResource)
            {
                return ChopTree(context);
            }

            Debug.Print($"No gathering behaviour implemented for worker and resource {resourceGatherTask.GetType()}");
            return BehaviourStatus.Failed;
        }
       
        #endregion
        #endregion
        #region resource pickup and dropoff
        private IBehaviour<UnitBTContext> DropOfResourcesSubtree => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Drop of resources")
                
                .Do("FindResourceDropoffPoint", FindResourceDropoffPoint)
                .Do("find path", FindPathToDestination)
                .Do("Follow path", FollowPath)
                .Do("DropOfResource", DepositResourcesAtResourceStore)
            .End()
        .Build();

        private bool UnitCarriesResources(UnitBTContext context)
        {
            if (Unit.Inventory.CurrentCapacity > 0)
            {
                return true;
            }
            return false;
        }

        public BehaviourStatus FindResourcePickupPoint(UnitBTContext context)
        {
            var task = context.AssignedTask as BuildBuildingTask;
            // Higher is better
            float buildingWithBestResourcesConstraint(StorageBuildingBase storageBuilding)
            {
                var overlappingType = storageBuilding.StoredResources.TypesOfResourcesStored & task.Building.Instance.BuildingResources.TypesOfResourcesRequired;

                float totalResourcesReq = 0f;
                float totalResourcesAtResourceStore = 0f;
                foreach (var t in overlappingType.GetActiveFlags())
                {
                    var amountRequired = task.Building.Instance.BuildingResources.RequiresOfResource(t);
                    totalResourcesReq += amountRequired;

                    var amountAvailable = storageBuilding.StoredResources.GetResourcesOfType(t);

                    totalResourcesAtResourceStore += MathF.Min(amountAvailable, amountRequired); // only the required amount matters
                }

                // now to order, we should take a function of the distance and the resource value the storage would bring.
                var percentageOfAllResources = totalResourcesAtResourceStore / totalResourcesReq;
                var distanceCoeff = storageBuilding.GlobalPosition.DistanceTo(Unit.GlobalPosition) * 2; // x2 since it has to walk twice, but mathematically i dont think this matters.
                var value = percentageOfAllResources / distanceCoeff;

                return value;
            }

            var closestResourceDeposit = this.Unit.BuildingManager.GetBuildings()
                .Where(b => b.Instance.IsResourceStorage)
                .Where(b => ((b.Instance as StorageBuildingBase).StoredResources.TypesOfResourcesStored & task.Building.Instance.BuildingResources.TypesOfResourcesRequired) > 0)
                .OrderBy(b => b.IsUnreachableCounter) //TODO: make this task based too
                .ThenByDescending(b => buildingWithBestResourcesConstraint(b.Instance as StorageBuildingBase))
                .FirstOrDefault();

            if (closestResourceDeposit == null)
            {
                return BehaviourStatus.Failed;
            }

            context.ResourceStorageBuilding = (StorageBuildingBase)closestResourceDeposit.Instance;

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus FindPathToResourcePickupPoint(UnitBTContext context)
        {
            context.Path = null;
            context.CurrentPathIndex = 0;

            var startCell = Unit.MapManager.WorldToCell(Unit.GlobalPosition);
            var goalCell = Unit.MapManager.WorldToCell(context.ResourceStorageBuilding.GlobalPosition);
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

        public BehaviourStatus PickUpResources(UnitBTContext context)
        {
            //TODO: this is only when building a building right now, it would be better to make it generic by keeping track of an instance of what the unit is picking up, and how much
            var buildingStoredResourceType = context.ResourceStorageBuilding.StoredResources.TypesOfResourcesStored;

            var task = context.AssignedTask as BuildBuildingTask;

            var requiredResourcesForBuilding = task.Building.Instance.BuildingResources.TypesOfResourcesRequired;

            if (requiredResourcesForBuilding.HasFlag(buildingStoredResourceType))
            {
                var amountRequired = task.Building.Instance.BuildingResources.RequiresOfResource(buildingStoredResourceType);

                var amountToPickup = Mathf.Min(amountRequired, Unit.Inventory.GetStorageCapacityLeft(buildingStoredResourceType));

                var amountTaken = context.ResourceStorageBuilding.StoredResources.RemoveResource(buildingStoredResourceType, amountToPickup);

                Unit.Inventory.AddResource(buildingStoredResourceType, amountTaken);

                return BehaviourStatus.Succeeded;
            }
            else // pickup point does not have the required resource
            {
                //happens if A: Resource pickup point became empty in the meanwhile
                // B: building is destroyed (TODO: take this into account)
                // C: no resource of this type required anymore
                return BehaviourStatus.Failed; //NOTE: do we need to fail in this situation? What i want is the unit to continue it's journey / find another resource which is needed
            }
        }

        public BehaviourStatus FindResourceDropoffPoint(UnitBTContext context)
        {
            var storagebuildings = from b in Unit.BuildingManager.GetBuildings()
                                   where b.Instance.BuildingCompleted && b.Instance.IsResourceStorage

                                   //orderby b.IsUnreachableCounter, b.GetNrOfAssignedUnits descending
                                   select b.Instance as StorageBuildingBase;

            var sortedStoragebuildings = from b in storagebuildings
                    where (b.StoredResources.TypesOfResourcesStored & Unit.Inventory.TypesOfResourcesStored) > 0
                    orderby _getWeightedResourceDropoffPointValue(b, Unit) descending
                    select b;


            var targetBuilding = sortedStoragebuildings.FirstOrDefault();
            //TODO: What if we don't have this resource in the entire kingdom, wanted behaviour would be is th


            if (targetBuilding == null)
            {
                return BehaviourStatus.Failed;
            }

            context.ResourceStorageBuilding = targetBuilding;
            context.Destination = Unit.MapManager.CellToWorld(context.ResourceStorageBuilding.Cell, centered: true);

            return BehaviourStatus.Succeeded;
        }

        private static float _getWeightedResourceDropoffPointValue(StorageBuildingBase storageBuilding, Unit unit)
        {
            var overlappingResourceTypes = storageBuilding.StoredResources.InputResourceTypes & unit.Inventory.TypesOfResourcesStored;

            var spaceLeft = storageBuilding.StoredResources.GetStorageCapacityLeft(overlappingResourceTypes);
            var amountNeededToStore = unit.Inventory.CurrentCapacity;//;.GetResourcesOfType(overlappingResourceTypes);

            var storageEfficientyValue = MathF.Min(spaceLeft / amountNeededToStore, 1);

            var distance = unit.GlobalPosition.DistanceSquaredTo(storageBuilding.GlobalPosition);

            return storageEfficientyValue / distance;
        }

        public BehaviourStatus DepositResourcesAtResourceStore(UnitBTContext context)
        {
            var building = context.ResourceStorageBuilding;

            foreach (var resourceType in Unit.Inventory.TypesOfResourcesStored.GetActiveFlags())
            {
                var amountRequired = building.StoredResources.GetStorageCapacityLeft(resourceType);
                if (amountRequired > 0)
                {
                    var amountRemovedFromUnit = Unit.Inventory.RemoveResource(resourceType, amountRequired);
                    var leftover = building.StoredResources.AddResource(resourceType, amountRemovedFromUnit);
                    if (leftover > 0)
                    {
                        Unit.Inventory.AddResource(resourceType, leftover);
                        GD.PushWarning($"'{Unit.UnitName}': WARNING: leftover resources detected, this should not happen since its calculated beforehands");
                    }
                }
            }

            return BehaviourStatus.Succeeded;
        }
        #endregion
        #region Deliver resources to building
        private IBehaviour<UnitBTContext> DeliverResourcesToBuildingSubtree => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Deliver resources to Building")
                //.Do("Find Delivery Target", this.FindDeliveryTarget)
                .Selector("Get resources if needed")
                    .Condition("Unit has resources", this.UnitHasResourcesForBuilding)
                    .Sequence("Pick up resources")
                        .Do("Find Resource Pickup Point", FindResourcePickupPoint)
                        .Do("Find Path To Resource Pickup Point", this.FindPathToResourcePickupPoint)
                        .Do("Move To ResourcePickupPoint", this.FollowPath)
                        .Do("Pick Up Resources", this.PickUpResources) //todo, if still has space, check if any other resources are also needed?
                    .End()
                .End()

                .Do("Find path to target", this.FindPathToDestination)
                .Do("Move To target", this.FollowPath)
                .Do("Deposit Resources", this.DeliverBuildingResources)
            .End()
        .Build();

        //public BehaviourStatus FindDeliveryTarget(UnitBTContext context)
        //{
        //    var buildingsOrdered = Unit.BuildingManager.GetBuildings()
        //        .Where(b => !b.Instance.BuildingCompleted && b.Instance.BuildingResources.RequiresResources)
        //        .OrderByDescending(b => b.GetNrOfAssignedUnits)
        //        .ThenBy(b => b.IsUnreachableCounter)
        //        .ThenBy(b => b.Instance.GlobalPosition.DistanceTo(Unit.GlobalPosition));

        //    BuildingDataObject targetBuilding = buildingsOrdered.FirstOrDefault();
        //    //TODO: What if we don't have this resource in the entire kingdom, wanted behaviour would be is that this task fails


        //    if (targetBuilding == null)
        //    {
        //        return BehaviourStatus.Failed;
        //    }

        //    var t = new DeliverBuildingResourcesToBuildingTask(targetBuilding);
        //    context.AssignedTask = t;
        //    context.Destination = t.TaskPosition;

        //    return BehaviourStatus.Succeeded;
        //}

        public bool UnitHasResourcesForBuilding(UnitBTContext context)
        {
            var t = context.AssignedTask as BuildBuildingTask;

            ResourceType unitHasResourcesForBuilding = (t.Building.Instance.BuildingResources.TypesOfResourcesRequired & Unit.Inventory.TypesOfResourcesStored);
            if (unitHasResourcesForBuilding > 0)
            {
                //GD.Print($"unit has resources for building of type {unitHasResourcesForBuilding}, inventory is: {Unit.Inventory.TypesOfResourcesStored}, wood:{Unit.Inventory.Wood}, stone: {Unit.Inventory.Stone}");

                return true;
            }
            return false;
        }

        public BehaviourStatus DeliverBuildingResources(UnitBTContext context)
        {
            var task = (BuildBuildingTask)context.AssignedTask;
            var building = task.Building.Instance;

            foreach (var resourceType in Unit.Inventory.TypesOfResourcesStored.GetActiveFlags())
            {
                var amountRequired = building.BuildingResources.RequiresOfResource(resourceType);
                if (amountRequired > 0)
                {
                    var amountRemovedFromUnit = Unit.Inventory.RemoveResource(resourceType, amountRequired);
                    var leftover = building.BuildingResources.AddResource(resourceType, amountRemovedFromUnit);
                    if (leftover > 0)
                    {
                        Unit.Inventory.AddResource(resourceType, leftover);
                        GD.PushWarning($"'{Unit.UnitName}': WARNING: leftover resources detected, this should not happen since its calculated beforehands");
                    }
                }
            }

            task.Building.IsUnreachableCounter = 0;

            return BehaviourStatus.Succeeded;
        }
        #endregion
    }
}
