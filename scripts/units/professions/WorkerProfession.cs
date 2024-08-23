using System;
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
using SacaSimulationGame.scripts.naturalResources.instances;
using SacaSimulationGame.scripts.buildings.storages;


namespace SacaSimulationGame.scripts.units.professions
{
    public class WorkerProfession : Profession
    {
        public WorkerProfession(Unit unit) : base(unit) { }
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

        private float _getWeightedBuildingToBuildValue(Unit unit, DeliverBuildingResourcesToBuildingTask task, float maxTaskValue)
        {
            float assignedUnitCoefficient = maxTaskValue * 0.5f;
            float prioritizeBuildingsWhichWaitForResourcesCoefficient = maxTaskValue * 0.5f;

            var nrOfAssignedUnitsValue = task.Building.GetNrOfAssignedUnits > 0 ? assignedUnitCoefficient : 0;

            // between 0 and 1
            float percentDiff = task.Building.Instance.BuildingResources.PercentageResourcesAquired - task.Building.Instance.BuildingPercentageComplete; // big is bad
            float amountOfResourcesStillRequiredValue = prioritizeBuildingsWhichWaitForResourcesCoefficient - (percentDiff * prioritizeBuildingsWhichWaitForResourcesCoefficient);

            //float distance = unit.GlobalPosition.DistanceTo(task.TaskPosition);

            float value = (nrOfAssignedUnitsValue + amountOfResourcesStillRequiredValue);

            return value;
        }


        private BehaviourStatus InitiateTaskSelector(UnitBTContext context)
        {
            context.TasksToChooseFrom = new();
            return BehaviourStatus.Succeeded;
        }

        private BehaviourStatus IncludeTaskManagerTasks(UnitBTContext context)
        {
            bool getAllTasksWithoutEncounteringUnfinishedOnes(List<UnitTask> inputTasks, out List<UnitTask> outputTasks)
            {
                outputTasks = new List<UnitTask>();

                foreach (var task in inputTasks)
                {
                    if (task.IsFinished)
                    {
                        this.Unit.GameManager.TaskManager.FinishTask(task);
                        // since its finished now, we have to retry the task selection
                        outputTasks = null;
                        return false;
                    }
                    outputTasks.Add(task);
                }

                return true;
            }

            try
            {
                List<UnitTask> tasksToAdd;
                int timeoutCount = 0;

                while (!getAllTasksWithoutEncounteringUnfinishedOnes(Unit.GameManager.TaskManager.GetTasks().ToList(), out tasksToAdd)) {
                    timeoutCount++;
                    if (timeoutCount > 100)
                    {
                        return BehaviourStatus.Failed; // just to prevent infinite loops in edge cases
                    }
                }

                context.TasksToChooseFrom.AddRange(tasksToAdd);
            }
            catch (Exception ex)
            {
                throw;
            }
            return BehaviourStatus.Succeeded;
        }
        private BehaviourStatus IncludeTransportResourcesToStorageTasks(UnitBTContext context)
        {

            return BehaviourStatus.Succeeded;
        }
        private IBehaviour<UnitBTContext> TaskExecutionSubtree => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Perform task")
                .Do("", InitiateTaskSelector)
                .Do("", IncludeTaskManagerTasks)
                .Do("", IncludeTransportResourcesToStorageTasks)
                //TODO: exclude finished tasks? or is it already happening?
                
                .Do("Select Best Task", SelectBestTask) // right now this will make it fail

                .Selector("Task selector")
                    .Sequence("TASK VALIDATION: Deliver Resources To building")
                        .Condition("", c => c.AssignedTask is DeliverBuildingResourcesToBuildingTask)
                        .Subtree(DeliverResourcesToBuildingSubtree) 
                    .End()
                    .Sequence("TASK VALIDATION: Natural Resource collection")
                        .Condition("", c => c.AssignedTask is NaturalResourceGatherTask)
                        .Subtree(NaturalResourceGatherSubtree)
                    .End()
                    .Sequence("TASK VALIDATION: Remove resources from resource building")
                        .Condition("", c => c.AssignedTask is PickupResourcesTask)
                        .Subtree(RemoveResourcesFromResourceBuilding)
                    .End()
                .End()
            .End()
        .Build();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Failed if task is finished, otherwise succeeded</returns>
        //private BehaviourStatus FinishTaskIfFinished(UnitBTContext context) {
        //    if (context.AssignedTask.IsFinished)
        //    {
        //        Unit.GameManager.TaskManager.FinishTask(context.AssignedTask); //TODO: will this not give reference issues? since finishtask might alter the UnitTaskQueue
        //        return BehaviourStatus.Failed;
        //    }

        //    return BehaviourStatus.Succeeded;
        //}

        /// <summary>
        /// Grades a task on its value, higher value is better
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private float GradeTaskValue(UnitTask task, Unit unit)
        {
            var maxTaskValue = 10;

            float value = float.MinValue;
            if(task is DeliverBuildingResourcesToBuildingTask bt)
            {
                value = _getWeightedBuildingToBuildValue(Unit, bt, maxTaskValue);
            }
            else if(task is NaturalResourceGatherTask rt)
            {
                // Its more important if 
                var percentLeft = rt.Resource.ResourceStorage.CurrentCapacity / rt.Resource.ResourceStorage.MaxCapacity;
                var multiplier = 0.5f + (percentLeft * 0.5f);

                if(task.ParentTask != null)
                {
                    multiplier = Mathf.Min(percentLeft + 0.3f, 1.0f); // give an bonus
                }
                value = maxTaskValue * multiplier;
            }
            else if( task is PickupResourcesTask pt)
            {
                value = GetResourceCollectionBuildingGrading(Unit, pt.Building);
            }

            var distance = Unit.GlobalPosition.DistanceTo(task.TaskPosition);

            return value / distance;
        }



        public BehaviourStatus SelectBestTask(UnitBTContext context)
        {
            var availableTasks = context.TasksToChooseFrom.ToList();

            if (availableTasks.Count == 0)
            {
                return BehaviourStatus.Failed;
            }

            var availableTasksOrdered = availableTasks
                .OrderBy(t => GradeTaskValue(t, Unit));

            var availableTask = availableTasks.First();

            context.AssignedTask = availableTask;
            context.Destination = availableTask.TaskPosition;

            return BehaviourStatus.Succeeded;
        }

        private BehaviourStatus SetDestinationToTaskLocation(UnitBTContext context)
        {
            context.Destination = context.AssignedTask.TaskPosition;

            return BehaviourStatus.Succeeded;
        }

        #region TASK: removeResourcesFromResourceBuildings

        /// <summary>
        /// 1. Find building which needs to be emptied 
        /// </summary>
        private IBehaviour<UnitBTContext> RemoveResourcesFromResourceBuilding => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Remove Resources from Resource Buildings")
                .Do("Find Path", FindPathToDestination)
                .Do("follow path", FollowPath)
                .Do("Pick up resource", PickUpResources)
            .End()
        .Build();

        public BehaviourStatus PickUpResources(UnitBTContext context)
        {
            var task = context.AssignedTask as PickupResourcesTask;
            var building = task.Building;

            bool succeed = false;
            foreach (var resourceType in building.StoredResources.OutputResourceTypes.GetActiveFlags())
            {
                var spaceLeft = Unit.Inventory.GetStorageCapacityLeft(resourceType);
                var amountTaken = building.StoredResources.RemoveResource(resourceType, spaceLeft);
                if (spaceLeft > 0)
                {
                    succeed = true;
                    Unit.Inventory.AddResource(resourceType, amountTaken);
                }
            }

            if (succeed)
            {
                GD.Print("pick up resources succeed");
                return BehaviourStatus.Succeeded;
            }

            else
            {
                GD.Print("pick up resources failed");
                return BehaviourStatus.Failed;
            }
        }


        /// <summary>
        /// Higher is better
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="building"></param>
        /// <param name="resourcesAvailable"></param>
        /// <returns></returns>
        private float GetResourceCollectionBuildingGrading(Unit unit, StorageBuildingBase building)
        {
            var storedResources = ((StorageBuildingBase)building).StoredResources;

            var spaceLeft = storedResources.GetStorageCapacityLeft(storedResources.OutputResourceTypes);
            var currentStorageAmount = storedResources.GetResourcesOfType(storedResources.OutputResourceTypes);

            var storedResourcesValue = Mathf.Sqrt(currentStorageAmount+1) - 1;
            var storageAlmostFullValue = Mathf.Clamp(( 5f / Mathf.Sqrt(spaceLeft+1)) - 1, 0, 1);

            float distance = unit.GlobalPosition.DistanceTo(building.GlobalPosition);

            float value = (storedResourcesValue + storageAlmostFullValue) / distance;

            return value;
        }

        //private BehaviourStatus FindResourceCollectingBuildingToEmpty(UnitBTContext context)
        //{
        //    var buildingsOrdered = Unit.BuildingManager.GetBuildings()
        //        .Where(b => b.Instance.BuildingCompleted && b.Instance.IsResourceStorage)
        //        .Select(b => (
        //            b,
        //            (StorageBuildingBase)b.Instance)
        //        )
        //        .Where(pair => {
        //            var ra = pair.Item2.StoredResources.GetResourcesOfType(pair.Item2.StoredResources.OutputResourceTypes);
        //            return pair.Item2.StorageStrategy == StorageStrategyEnum.EmptyAllResources && ra > 0;
        //        }
        //        )
        //        .OrderByDescending(pair => GetResourceCollectionBuildingGrading(Unit, pair.b))
        //        .ToList();
                
        //    if(buildingsOrdered.Count == 0)
        //    {
        //        GD.Print($" No resource pickup building found");
        //        return BehaviourStatus.Failed;
        //    }

        //    var buildingPair = buildingsOrdered.First();

        //    // add it to task
        //    context.AssignedTask = new PickupResourcesTask(buildingPair.b.Instance);
        //    context.Destination = context.AssignedTask.TaskPosition;

        //    return BehaviourStatus.Succeeded;
        //}

        #endregion

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
            if(resourceGatherTask.Resource is RockResource rockResource)
            {
                return GatherRock(context);
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
            var task = context.AssignedTask as DeliverBuildingResourcesToBuildingTask;
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

        public BehaviourStatus PickUpResourcesFromBuilding(UnitBTContext context)
        {
            //TODO: this is only when building a building right now, it would be better to make it generic by keeping track of an instance of what the unit is picking up, and how much
            var sb = context.ResourceStorageBuilding;
            
            StorageBase sr;
            try
            { 
                sr = sb.StoredResources;
            }
            catch(Exception ex)
            {
                GD.PushWarning($"{Unit.UnitName}: Null error occured in {sb.Type}, with resourcestore ({sb?.StoredResources?.InputResourceTypes}, {sb?.StoredResources?.OutputResourceTypes}), and ex {ex.Message}");
                throw;
            }
            var buildingStoredResourceType = sr.TypesOfResourcesStored; //TODO: FIXME, this currently can be None (undefiend) making it crash

            if(buildingStoredResourceType == 0)
            {
                return BehaviourStatus.Failed; // TODO: can we chat this during the walking already ,so that it happens during the follow path when the task is already completed / resource deposit is empty??
            }

            var task = context.AssignedTask as DeliverBuildingResourcesToBuildingTask;

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


            bool validResourceBuildingCondition(StorageBuildingBase b)
            {
                if (b.StorageStrategy != StorageStrategyEnum.KeepResourcesUntilNeeded) return false;

                //conditions: either the stored resource type is None but it allows for the
                var overlappingResourceType = b.StoredResources.InputResourceTypes & Unit.Inventory.TypesOfResourcesStored;
                if (overlappingResourceType == 0)
                {
                    return false;
                }

                if(b.StoredResources.GetStorageCapacityLeft(overlappingResourceType) == 0)
                {
                    return false;
                }         

                return true;
            }


            var sortedStoragebuildings = from b in storagebuildings
                    where validResourceBuildingCondition(b)
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
                        .Do("Pick Up Resources", this.PickUpResourcesFromBuilding) //todo, if still has space, check if any other resources are also needed?
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
            var t = context.AssignedTask as DeliverBuildingResourcesToBuildingTask;

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
            var task = (DeliverBuildingResourcesToBuildingTask)context.AssignedTask;
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


        protected BehaviourStatus GatherRock(UnitBTContext context)
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
    }
}
