using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.units.tasks;
using Windows.Services.Maps;

namespace SacaSimulationGame.scripts.units.professions
{
    public class BuilderProfession(Unit unit) : Profession(unit)
    {
        protected override BuildingType ProfessionBuildingType => BuildingType.None;
        protected override float ActivitySpeedBaseline => 3;

        private readonly float _waittimeUntilBuildingTimeout = 15;
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
            var buildingTasks = Unit.GameManager.TaskManager.GetTasks()
                
                .Where(t => t is BuildBuildingTask)
                .Select(t => t as BuildBuildingTask)
                .OrderBy(t => t.Building.IsUnreachableCounter)
                .ThenByDescending(t => t.Building.Instance.BuildingResources.PercentageResourcesAquired - t.Building.Instance.BuildingPercentageComplete)
                .ThenBy(t => t.Building.Instance.GlobalPosition.DistanceTo(Unit.GlobalPosition));

            BuildBuildingTask targetTask = null;
            foreach (var task in buildingTasks)
            {
                if (task.Building.AssignUnit(Unit))
                {
                    targetTask = task;
                    break;
                }
            }

            if (targetTask == null)
            {
                return BehaviourStatus.Failed;
            }


            context.AssignedTask = targetTask;
            //context.Building = targetTask;
            context.Destination = Unit.MapManager.CellToWorld(targetTask.Building.Instance.Cell, centered: true);

            GD.Print($"'{Unit.UnitName}': target building is located at {context.Destination}");

            return BehaviourStatus.Succeeded;
        }
        
        public BehaviourStatus BuildBuilding(UnitBTContext context)
        {
            var t = context.AssignedTask as BuildBuildingTask;
            
            if(t.Building.Instance.BuildingPercentageComplete >= t.Building.Instance.BuildingResources.PercentageResourcesAquired)
            {
                //waiting for resources
                context.WaitingTime += context.Delta;
                if (context.WaitingTime > _waittimeUntilBuildingTimeout)
                {
                    t.Building.IsUnreachableCounter++;
                    t.Building.UnassignUnit(Unit);
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
                t.Building.IsUnreachableCounter = 0;
                var finished = t.Building.Instance.AddBuildingProgress(context.Delta * GetActivitySpeedCoefficient());

                if (finished)
                {
                    t.Building.UnassignUnit(Unit); //TODO: what if the task gets canceled? i also want to it clear then.
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
                GD.Print($"'{Unit.UnitName}': No path could be found to the destination for building task");
                var t = context.AssignedTask as BuildBuildingTask;
                t.Building.IsUnreachableCounter++;
                t.Building.UnassignUnit(Unit);
            }

            return result;
        }
    }
}
