using System.Collections.Generic;
using Godot;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.managers
{
    public interface INaturalResourceManager
    {
        GameManager GameManager { get; set; }
        List<NaturalResource> NaturalResources { get; set; }
        PackedScene TreeModel { get; set; }
        bool[,] OccupiedCells { get; }
        bool AddResource(Vector3 position, NaturalResourceType type);
        bool RemoveResource(NaturalResource resource);
        IEnumerable<NaturalResource> GetResourcesAtCell(Vector2I cell);
    }
}