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
using Windows.Services.Maps;
using Windows.Web.Http.Headers;

namespace SacaSimulationGame.scripts.units.professions
{
    public class LumberjackProfession(Unit unit) : Profession(unit)
    {
        private IBehaviour<UnitBTContext> MoveToProfessionBuildingAndDropoffResources => FluentBuilder.Create<UnitBTContext>()
            .Sequence("Move to Profession Building And Drop off Resources")
                .Do("Set destination", SetDestinationToProfessionBuilding)
                .Do("Find path", FindPathToDestination)
                .Do("Follow path", FollowPath)
                .Do("Drop off Resources", DropOfResourcesAtProfessionBuilding)
            .End()
            .Build();
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

        BehaviourStatus SetDestinationToProfessionBuilding(UnitBTContext context)
        {
            context.Destination = ProfessionBuilding.GlobalPosition;

            return BehaviourStatus.Succeeded;
        }

        BehaviourStatus DropOfResourcesAtProfessionBuilding(UnitBTContext context)
        {
            var resourcesToCheck = new List<ResourceType>();
            var t = Unit.Inventory.TypesOfResourcesStored;
            if (t.HasFlag(ResourceType.Wood)) resourcesToCheck.Add(ResourceType.Wood);
            if (t.HasFlag(ResourceType.Stone)) resourcesToCheck.Add(ResourceType.Stone);

            foreach (var resourceType in resourcesToCheck)
            {
                var storageSpace = ProfessionBuilding.StoredResources.GetStorageSpaceLeft(resourceType);

                var amountRemoved = Unit.Inventory.RemoveResource(resourceType, storageSpace);

                if (amountRemoved > 0)
                {
                    var leftOver = ProfessionBuilding.StoredResources.AddResource(resourceType, amountRemoved);
                    if (leftOver > 0) 
                    {
                        GD.PushWarning($"Resource leftover of {leftOver} of resource {resourceType}, this should never happen here.");
                        Unit.Inventory.AddResource(resourceType, leftOver);
                    }
                }
            }

            return BehaviourStatus.Succeeded;
        }

        BehaviourStatus ChopTree(UnitBTContext context) {

            var resourcesToTake = (float)context.Delta * Unit.Stats.ActivitySpeedMultiplier;

            var (resourcesTaken, resourceType) = context.AssignedResource.CollectResource(resourcesToTake);

            if(resourcesTaken == 0 || resourceType == default)
            {
                //TODO: this happens for some reason, debuh
                GD.Print("no resource could be found");
                return BehaviourStatus.Succeeded; // finished chopping
            }

            Unit.Inventory.AddResource(resourceType, resourcesTaken);

            context.WaitingTime += context.Delta;
            var timeoutTime = 30;

            //failsafe:
            if (context.WaitingTime > timeoutTime) {
                return BehaviourStatus.Failed;
            }
            

            if(context.AssignedResource.GetNrOfResourcesLeft() == 0)
            {
                return BehaviourStatus.Succeeded;
            }
            if(Unit.Inventory.GetStorageSpaceLeft(resourceType) == 0)
            {
                return BehaviourStatus.Succeeded;
            }

            return BehaviourStatus.Running;
        }

        BehaviourStatus AssignCuttableTreeInRadius(UnitBTContext context)
        {
            var collectionRadius = 12;
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

            context.AssignedResource = closestResource;
            context.Destination = closestResource.GlobalPosition;

            return BehaviourStatus.Succeeded;
        }

        BehaviourStatus PlantTree(UnitBTContext context)
        {
            var treePlantingDuration = 7;

            context.WaitingTime += context.Delta;
            if(context.WaitingTime <= treePlantingDuration / Unit.Stats.ActivitySpeedMultiplier)
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

            ProfessionBuilding = closestBuilding.Instance as StorageBuildingBase;

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
