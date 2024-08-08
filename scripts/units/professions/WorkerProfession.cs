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

namespace SacaSimulationGame.scripts.units.professions
{
    public class WorkerProfession(Unit unit) : Profession(unit)
    {
        //TODO: what if unit has wrong resources?
        //TODO: FindBuildingWithUnitsResources
        //TODO: DropOfResourcesAtDropoffPoint
        //TODO: PickUpResources
        //TODO: if building requires more of the resource, and inventory is not full, repeat by going to next pickup point
        private readonly Random _rnd = new();

        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            //1. find target
            //if has resources: drop of resources
            //else: pick up resources
            return FluentBuilder.Create<UnitBTContext>()
                .Selector("root")
                    .Sequence("Deliver resources to Building")
                        .Do("Find Delivery Target", this.FindDeliveryTarget)
                        .Selector("Get resources if needed")
                            .Condition("Unit has resources", this.UnitHasResourcesForBuilding)
                            .Sequence("Pick up resources")
                                .Do("Find Resource Pickup Point", FindResourcePickupPoint)
                                .Do("Find Path To Resource Pickup Point", this.FindPathToResourcePickupPoint)
                                .Do("Move To ResourcePickupPoint",this.FollowPath)
                                .Do("Pick Up Resources", this.PickUpResources) //todo, if still has space, check if any other resources are also needed?
                            .End()
                        .End()
                        
                        .Do("Find path to target", this.FindPathToDestination)
                        .Do("Move To target", this.FollowPath)
                        .Do("Deposit Resources", this.DepositResources)
                    .End()
                    //TODO: subtree to drop of resources in case it doesnt have a job. This also needs to be done whenever it needs to pick up a new resource for a job but doesnt have space for it.
                    .Subtree(IdleBehaviourTree)
                .End()
                .Build();
        }

        public BehaviourStatus PickUpResources(UnitBTContext context) {
            //TODO: this is only when building a building right now, it would be better to make it generic by keeping track of an instance of what the unit is picking up, and how much
            var buildingStoredResourceType = context.ResourcePickupBuilding.StoredResources.TypesOfResourcesStored;

            var requiredResourcesForBuilding = context.Building.Instance.BuildingResources.TypesOfResourcesRequired;

            if (requiredResourcesForBuilding.HasFlag(buildingStoredResourceType))
            {
                var amountRequired = context.Building.Instance.BuildingResources.RequiresOfResource(buildingStoredResourceType);

                var amountToPickup = Mathf.Min(amountRequired, Unit.Inventory.GetStorageSpaceLeft(buildingStoredResourceType));

                var amountTaken = context.ResourcePickupBuilding.StoredResources.RemoveResource(buildingStoredResourceType, amountToPickup);

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

        public BehaviourStatus FindPathToResourcePickupPoint(UnitBTContext context)
        {
            context.Path = null;
            context.CurrentPathIndex = 0;

            var startCell = Unit.MapManager.WorldToCell(Unit.GlobalPosition);
            var goalCell = Unit.MapManager.WorldToCell(context.ResourcePickupBuilding.GlobalPosition);
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

        public BehaviourStatus FindResourcePickupPoint(UnitBTContext context)
        {
            // Higher is better
            float buildingWithBestResourcesConstraint(StorageBuildingBase storageBuilding)
            {
                var overlappingType = storageBuilding.StoredResources.TypesOfResourcesStored & context.Building.Instance.BuildingResources.TypesOfResourcesRequired;

                float totalResourcesReq = 0f;
                float totalResourcesAtResourceStore = 0f;
                foreach(var t in overlappingType.GetActiveFlags())
                {
                    var amountRequired = context.Building.Instance.BuildingResources.RequiresOfResource(t);
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
                .Where(b => ((b.Instance as StorageBuildingBase).StoredResources.TypesOfResourcesStored & context.Building.Instance.BuildingResources.TypesOfResourcesRequired) > 0)
                .OrderBy(b => b.IsUnreachableCounter)
                .ThenByDescending(b => buildingWithBestResourcesConstraint(b.Instance as StorageBuildingBase))
                .FirstOrDefault();

            if (closestResourceDeposit == null) {
       
                return BehaviourStatus.Failed; 
            }

            context.ResourcePickupBuilding = (StorageBuildingBase)closestResourceDeposit.Instance;

            return BehaviourStatus.Succeeded;
        }
        public bool UnitHasResourcesForBuilding(UnitBTContext context)
        {
            ResourceType unitHasResourcesForBuilding = (context.Building.Instance.BuildingResources.TypesOfResourcesRequired & Unit.Inventory.TypesOfResourcesStored);
            if(unitHasResourcesForBuilding > 0)
            {
                //GD.Print($"unit has resources for building of type {unitHasResourcesForBuilding}, inventory is: {Unit.Inventory.TypesOfResourcesStored}, wood:{Unit.Inventory.Wood}, stone: {Unit.Inventory.Stone}");
                
                return true;
            }
            return false;
        }


        private static readonly ResourceType[] _possibleResourceTypes = [ResourceType.Wood, ResourceType.Stone];//Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>().Where(r=>r != ResourceType.None).ToArray();

        public BehaviourStatus DepositResources(UnitBTContext context)
        {
            var building = context.Building.Instance;

            foreach (var resourceType in _possibleResourceTypes)
            {
                var amountRequired = building.BuildingResources.RequiresOfResource(resourceType);
                if (amountRequired > 0)
                {
                    var amountRemovedFromUnit = Unit.Inventory.RemoveResource(resourceType, amountRequired);
                    var leftover = building.BuildingResources.AddResource(resourceType, amountRemovedFromUnit);
                    if (leftover > 0)
                    {
                        Unit.Inventory.AddResource(resourceType, leftover);
                        GD.PushWarning($"Warning: leftover resources detected, this should not happen since its calculated beforehands");
                    }
                }
            }

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus FindDeliveryTarget(UnitBTContext context)
        {
            var buildingsOrdered = Unit.BuildingManager.GetBuildings().Where(b => !b.Instance.BuildingCompleted && b.Instance.BuildingResources.RequiresResources)
                .OrderBy(b => b.IsUnreachableCounter)
                .ThenByDescending(b => b.GetNrOfAssignedUnits);
                //.ThenBy(b => b.Building.GlobalPosition.DistanceTo(Unit.GlobalPosition));

            BuildingDataObject targetBuilding = buildingsOrdered.FirstOrDefault();
            //TODO: What if we don't have this resource in the entire kingdom, wanted behaviour would be is th

            //foreach (var building in buildingsOrdered)
            //{
            //    if (building.AssignUnit(Unit))
            //    {
            //        targetBuilding = building;
            //        break;
            //    }
            //}

            if (targetBuilding == null)
            {
                return BehaviourStatus.Failed;
            }

            context.Building = targetBuilding;
            context.Destination = Unit.MapManager.CellToWorld(context.Building.Instance.Cell, centered: true);

            return BehaviourStatus.Succeeded;
        }
    }
}
