using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.managers;
using Windows.Services.Maps;
using Windows.Web.Http.Headers;

namespace SacaSimulationGame.scripts.units.professions
{
    public class LumberjackProfession(Unit unit) : Profession(unit)
    {
        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            return FluentBuilder.Create<UnitBTContext>()
                .Selector("root")
                    .Sequence("Main behaviour")
                        .Selector("Find profession building if needed")
                            .Condition("HasAssignedBuilding", HasAssignedBuilding)
                            .Do("Find profession building", FindProfessionBuilding)
                            //TODO: move to profession building
                        .End()
                        .RandomSelector("Action Selection")
                            .Sequence("Plant Tree")
                                .Do("Find Suitable planting position", FindSuitablePlantingLocation)
                                .Do("Find path to building", FindPathToDestination)
                                .Do("Move To Building", FollowPath)
                                .Do("Plant Tree", PlantTree)
                            .End()
                            //.Sequence("Chop tree")
                                
                            //.End()
                        .End()
                    .End()
                    .Subtree(IdleBehaviourTree)
                .End()
                .Build();
        }

        BehaviourStatus PlantTree(UnitBTContext context)
        {
            var treePlantingDuration = 2;

            context.WaitingTime += context.Delta;
            if(context.WaitingTime <= treePlantingDuration)
            {
                return BehaviourStatus.Running;
            }

            //Unit.GameManager.
            //TODO: resource manager which allows planting trees and stuff
            if(Unit.GameManager.NaturalResourceManager.AddResource(context.Destination, NaturalResourceType.Tree))
            {
                context.WaitingTime = 0;
                return BehaviourStatus.Succeeded;
            }
            else
            {
                context.WaitingTime = 0;
                GD.Print("failed to plant tree");
                return BehaviourStatus.Failed;
            }


        }

        bool HasAssignedBuilding(UnitBTContext context)
        {
            if(this.ProfessionBuilding == null) return false;
            //TODO: what if a building is destroyed? this has to be taken into account
            return true;
        }
        BehaviourStatus FindProfessionBuilding(UnitBTContext context)
        {
            var closestBuilding = Unit.BuildingManager.GetBuildings()
                .Where(b => b.Instance.Type == BuildingType.Lumberjack && b.Instance.WorkingEmployees.Count < b.Instance.MaxNumberOfEmployees)
                .OrderBy(b => b.Instance.WorkingEmployees.Count)
                .ThenBy(b => Unit.GlobalPosition.DistanceTo(b.Instance.GlobalPosition))
                .FirstOrDefault();

            if(closestBuilding == null) return BehaviourStatus.Failed;

            ProfessionBuilding = closestBuilding.Instance;

            return BehaviourStatus.Succeeded;
        }

        BehaviourStatus FindSuitablePlantingLocation(UnitBTContext context)
        {
            var rand = new Random();

            var dir = new Vector3(rand.Next(-20, 20), 0, rand.Next(-20, 20));

            var newPos = Unit.GlobalPosition + dir;

            context.Destination = newPos;
            return BehaviourStatus.Succeeded;
        }
    }
}
