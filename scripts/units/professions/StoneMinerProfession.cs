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
    public class StoneMinerProfession : Profession
    {
        public StoneMinerProfession(Unit unit) : base(unit) { }

        protected override BuildingType ProfessionBuildingType { get; } = BuildingType.StoneMine;

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
                            //.Sequence("Plant Tree")
                            //    .Do("Find Suitable planting position", FindSuitablePlantingLocation)
                            //    .Do("Find path to tree planting site", FindPathToDestination)
                            //    .Do("Move To tree planting site", FollowPath)
                            //    .Do("Plant Tree", PlantTree)
                            //.End()
                            //.Sequence("Chop tree")
                            //    .Do("Check if cuttable tree in region", AssignCuttableTreeInRadius)
                            //    .Do("Find path to building", FindPathToDestination)
                            //    .Do("Move To tree planting site", FollowPath)
                            //    .Do("Chop Tree", ChopTree)
                                //drop of resources
                            //.End()
                            .AlwaysFail("fail")
                        .End()
                    .End()
                    .Subtree(IdleBehaviourTree)
                .End()
            .Build();
        }
    }
}
