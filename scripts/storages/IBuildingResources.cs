using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.buildings
{
    public interface IBuildingResources
    {
        float CurrentStone { get; }
        float CurrentWood { get; }
        float PercentageResourcesAquired { get; }
        float RequiredStone { get; set; }
        float RequiredWood { get; set; }
        bool RequiresResources { get; }
        ResourceType TypesOfResourcesRequired { get; set; }

        float AddResource(ResourceType resourceType, float amount);
        float RequiresOfResource(ResourceType resourceType);
        void ShowResources(bool visible);
    }
}