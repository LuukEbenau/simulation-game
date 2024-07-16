using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using SacaSimulationGame.scripts.buildings;
using WinRT;

namespace SacaSimulationGame.scripts.units
{
    public partial class Builder : Unit
    {
        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            var idleTree = FluentBuilder.Create<UnitBTContext>()
                .Sequence("Idle Sequence")
                    .Do("Idle", this.DoNothingSequence)
                .End()
                .Build();

            return FluentBuilder.Create<UnitBTContext>()
                .Selector("root")
                    .Sequence("Move to and build building")
                        .Do("Find Building To Build", this.FindBuildingToBuild)
                        .Do("Find path to building", this.FindPathToBuilding)
                        .Do("Move To Building", this.MoveToDestination)
                        .Do("Build Building", this.BuildBuilding)
                    .End()
                    .Subtree(idleTree)
                .End()
                .Build();
        }

        private BehaviourStatus IsInBuildingDistance(UnitBTContext context)
        {
            var buildingPos = GameManager.MapManager.CellToWorld(context.Building.Building.Cell, centered: true);

            if (GlobalPosition.DistanceTo(buildingPos) <= 1.0)
            {
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Failed;
        }


        public BehaviourStatus FindBuildingToBuild(UnitBTContext context)
        {
            var buildingsOrdered = BuildingManager.GetBuildings().Where(b=>!b.Building.BuildingCompleted)
                .OrderBy(b=>b.IsUnreachableCounter)
                .ThenBy(b => b.Building.GlobalPosition.DistanceTo(GlobalPosition));

            BuildingDataObject targetBuilding = null;
            foreach (var building in buildingsOrdered) {
                if (building.AssignUnit(this))
                {
                    targetBuilding = building;
                    break;
                }
            }

            if(targetBuilding == null)
            {
                return BehaviourStatus.Failed;
            }

            context.Building = targetBuilding;
            context.Destination = MapManager.CellToWorld(context.Building.Building.Cell, centered: true);

            return BehaviourStatus.Succeeded;
        }

        public BehaviourStatus BuildBuilding(UnitBTContext context)
        {
            var finished = context.Building.Building.AddBuildingProgress(context.Delta);

            if (finished)
            {
                context.Building.UnassignUnit(this); //TODO: what if the task gets canceled? i also want to it clear then.
                return BehaviourStatus.Succeeded;
            }
            else
            {
                return BehaviourStatus.Running;
            }
        }

        public BehaviourStatus FindPathToBuilding(UnitBTContext context)
        {
            var result = FindPathToDestination(context);
            if (result == BehaviourStatus.Failed)
            {
                context.Building.IsUnreachableCounter++;
            }

            return result;
        }
    }
}
