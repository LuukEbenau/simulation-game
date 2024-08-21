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
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.units.tasks;


namespace SacaSimulationGame.scripts.units.professions
{
    public class LumberjackProfession : Profession
    {
        public LumberjackProfession(Unit unit) : base(unit) { }

        private const float collectionRadius = 12.5f;
        protected const float treePlantingDuration = 7f;

        protected override BuildingType ProfessionBuildingType { get; } = BuildingType.Lumberjack;

        protected override float ActivitySpeedBaseline => 3; 

        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            return FluentBuilder.Create<UnitBTContext>()
                .Selector("root")
                    .Sequence("Main behaviour")
                        .Selector("Find profession building if needed")
                            .Condition("HasAssignedBuilding", HasProfessionBuilding)
                            .Do("Find profession building", FindProfessionBuilding)
                        .End()
                        .Subtree(MoveToProfessionBuildingAndDropoffResources)
                        //actions
                        .RandomSelector("Action Selection")
                            .Sequence("Plant Tree")
                                .Do("Find Suitable planting position", FindSuitablePlantingLocation)
                                .Do("Find path to tree planting site", FindPathToDestination)
                                .Do("Move To tree planting site", FollowPath)
                                .Do("Plant Tree", PlantTree)
                            .End()
                            .Sequence("Chop tree")
                                .Do("Check if cuttable tree in region", AssignCuttableTreeInRadius)
                                .Do("Find path to building", FindPathToDestination)
                                .Do("Move To tree planting site", FollowPath)
                                .Do("Chop Tree", ChopTree)
                                //drop of resources
                            .End()
                        .End()
                    .End()
                    .Subtree(IdleBehaviourTree)
                .End()
                .Build();
        }

        BehaviourStatus AssignCuttableTreeInRadius(UnitBTContext context)
        {
            var closestResource = Unit.GameManager.NaturalResourceManager.NaturalResources
                .Select(r => (r, r.GlobalPosition.DistanceTo(ProfessionBuilding.GlobalPosition)))
                .Where(r => r.Item2 < Unit.GameManager.MapManager.CellSize.X * collectionRadius)
                .OrderBy(r=>r.Item2)
                .Select(r=>r.r)
                .FirstOrDefault();

            if(closestResource == null)
            {
                return BehaviourStatus.Failed;
            }

            context.AssignedTask = new NaturalResourceGatherTask(closestResource);

            //context.AssignedResource = closestResource;
            context.Destination = closestResource.GlobalPosition;

            return BehaviourStatus.Succeeded;
        }

        BehaviourStatus PlantTree(UnitBTContext context)
        {
            context.WaitingTime += context.Delta;
            if(context.WaitingTime <= treePlantingDuration / GetActivitySpeedCoefficient())
            {
                return BehaviourStatus.Running;
            }

            if(Unit.GameManager.NaturalResourceManager.AddResource(context.Destination, NaturalResourceType.Tree))
            {
                context.WaitingTime = 0;
                return BehaviourStatus.Succeeded;
            }
            else
            {
                context.WaitingTime = 0;
                GD.Print($"'{Unit.UnitName}': failed to plant tree");
                return BehaviourStatus.Failed;
            }
        }




        BehaviourStatus FindSuitablePlantingLocation(UnitBTContext context)
        {
            int maxtries = 40;
            for (int i = 0; i < maxtries; i++) {
                var rand = new Random();
                var dir = new Vector3(rand.Next(-20, 20), 0, rand.Next(-20, 20));
                var newPos = Unit.GlobalPosition + dir;

                var cell = Unit.MapManager.WorldToCell(newPos);
                if (!this.Unit.MapManager.GetCellOccupation(cell).IsOccupied)
                {
                    context.Destination = newPos;
                    return BehaviourStatus.Succeeded;
                }
            }

            return BehaviourStatus.Failed;
        }
    }
}
