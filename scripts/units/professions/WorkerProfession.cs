using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree.FluentBuilder;
using BehaviourTree;
using Godot;
using SacaSimulationGame.scripts.buildings;

namespace SacaSimulationGame.scripts.units.professions
{
    public class WorkerProfession(Unit unit) : Profession(unit)
    {
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
                        .Condition("Unit has resources", this.UnitHasResourcesForBuilding)
                        .Do("Find path to target", this.FindPathToDestination)
                        .Do("Move To target", this.MoveToDestination)
                        .Do("Deposit Resources", this.DepositResources)
                    .End()
                .End()
                .Build();
        }

        //TODO: FindBuildingWithUnitsResources
        //TODO: DropOfResourcesAtDropoffPoint
        //TODO: PickUpResources

        public bool UnitHasResourcesForBuilding(UnitBTContext context)
        {
            //TODO: what if unit has wrong resources?
            var building = context.Building.Building;
            if (building.BuildingResources.RequiresOfResource(ResourceType.Wood)>0 && Unit.Inventory.Wood > 0)
            {
                return true;
            }
            if (building.BuildingResources.RequiresOfResource(ResourceType.Stone)>0 && Unit.Inventory.Stone > 0)
            {
                return true;
            }

            return false;
        }


        private static readonly ResourceType[] _possibleResourceTypes = Enum.GetValues(typeof(ResourceType)).Cast<ResourceType>().ToArray();

        public BehaviourStatus DepositResources(UnitBTContext context)
        {
            var building = context.Building.Building;

            foreach (var resourceType in _possibleResourceTypes)
            {
                var amountRequired = building.BuildingResources.RequiresOfResource(resourceType);
                if (amountRequired > 0)
                {
                    var amountRemovedFromUnit = Unit.Inventory.RemoveResource(resourceType, amountRequired);
                    var leftover = building.BuildingResources.Deposit(resourceType, amountRemovedFromUnit);
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
            var buildingsOrdered = Unit.BuildingManager.GetBuildings().Where(b => !b.Building.BuildingCompleted && b.Building.BuildingResources.RequiresResources)
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
            context.Destination = Unit.MapManager.CellToWorld(context.Building.Building.Cell, centered: true);

            return BehaviourStatus.Succeeded;
        }
    }
}
