﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Godot;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.units.professions.misc;
using Windows.Services.Maps;

namespace SacaSimulationGame.scripts.units.professions
{
    public class BuilderProfession(Unit unit) : Profession(unit)
    {
        private readonly float _waittimeUntilBuildingTimeout = 8;
        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            return FluentBuilder.Create<UnitBTContext>()
                .Selector("root")
                    .Sequence("Move to and build building")
                        .Do("Find Building To Build", this.FindBuildingToBuild)
                        .Do("Find path to building", this.FindPathToBuilding)
                        .Do("Move To Building", this.FollowPath)
                        .Do("Build Building", this.BuildBuilding)
                    .End()
                    .Subtree(this.IdleBehaviourTree)
                .End()
                .Build();
        }

        public BehaviourStatus FindBuildingToBuild(UnitBTContext context)
        {
            var buildingsOrdered = from b in Unit.BuildingManager.GetBuildings()
                    where !b.Instance.BuildingCompleted
                    orderby b.IsUnreachableCounter ascending,
                            b.Instance.BuildingResources.PercentageResourcesAquired - b.Instance.BuildingPercentageComplete descending,
                            b.Instance.GlobalPosition.DistanceTo(Unit.GlobalPosition) ascending
                    select b;

            BuildingDataObject targetBuilding = null;
            foreach (var building in buildingsOrdered)
            {
                if (building.AssignUnit(Unit))
                {
                    //GD.Print($"found target building to build {targetBuilding}");
                    targetBuilding = building;
                    break;
                }
            }

            if (targetBuilding == null)
            {
                return BehaviourStatus.Failed;
            }

            context.Building = targetBuilding;
            context.Destination = Unit.MapManager.CellToWorld(context.Building.Instance.Cell, centered: true);

            //GD.Print($"target building is located at {context.Destination}");

            return BehaviourStatus.Succeeded;
        }
        
        public BehaviourStatus BuildBuilding(UnitBTContext context)
        {
            if(context.Building.Instance.BuildingPercentageComplete >= context.Building.Instance.BuildingResources.PercentageResourcesAquired)
            {
                //waiting for resources
                context.WaitingTime += context.Delta;
                if (context.WaitingTime > _waittimeUntilBuildingTimeout)
                {
                    context.Building.IsUnreachableCounter++;
                    context.Building.UnassignUnit(Unit);
                    return BehaviourStatus.Failed;
                }
                else
                {
                    return BehaviourStatus.Running;
                }
            }
            else
            {
                context.WaitingTime = 0;
                var finished = context.Building.Instance.AddBuildingProgress(context.Delta * Unit.Stats.ActivitySpeedMultiplier);

                if (finished)
                {
                    context.Building.UnassignUnit(Unit); //TODO: what if the task gets canceled? i also want to it clear then.
                    return BehaviourStatus.Succeeded;
                }
                else
                {
                    return BehaviourStatus.Running;
                }
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
