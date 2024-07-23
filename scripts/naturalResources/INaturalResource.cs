namespace SacaSimulationGame.scripts.naturalResources
{
    public interface INaturalResource
    {
        (float amount, ResourceType type) CollectResource(float amount);
        float GetNrOfResourcesLeft();
    }
}