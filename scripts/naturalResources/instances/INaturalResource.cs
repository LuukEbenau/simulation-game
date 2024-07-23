namespace SacaSimulationGame.scripts.naturalResources.instances
{
    public interface INaturalResource
    {
        (float amount, ResourceType type) CollectResource(float amount);
        float GetNrOfResourcesLeft();
    }
}