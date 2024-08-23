using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.naturalResources.instances;

namespace SacaSimulationGame.scripts.managers
{
    public partial class NaturalResourceManager : Node3D, INaturalResourceManager
    {
        [Export] public PackedScene TreeModel { get; set; }
        [Export] public PackedScene RockModel { get; set; }
        public List<NaturalResource> NaturalResources { get; set; } = new List<NaturalResource> { };

        public GameManager GameManager { get; set; }

        private bool[,] _occupiedCells;
        public bool[,] OccupiedCells => _occupiedCells ??= new bool[GameManager.MapManager.MapWidth, GameManager.MapManager.MapLength];

        public override void _Ready()
        {
            base._Ready();
            GameManager = GetParent<GameManager>();

        }

        public bool RemoveResource(NaturalResource resource)
        {
            if (NaturalResources.Remove(resource))
            {
                OccupiedCells[resource.Cell.X, resource.Cell.Y] = false;
                RemoveChild(resource);
                return true;
            }

            return false;
        }

        public IEnumerable<NaturalResource> GetResourcesAtCell(Vector2I cell) { 
            foreach(var resource in NaturalResources)
            {
                if(resource.Cell == cell)
                {
                    yield return resource;
                }
            }
        }

        public bool AddResource(Vector3 position, NaturalResourceType type)
        {
            var cell = this.GameManager.MapManager.WorldToCell(position);

            var cellOccupation = GameManager.MapManager.GetCellOccupation(cell);
            if (cellOccupation.IsOccupied)
            {
                return false;
            }

            NaturalResource instance;
            if (type == NaturalResourceType.Tree)
            {
                instance = TreeModel.Instantiate<TreeResource>();
            }
            else if(type == NaturalResourceType.Stone)
            {
                instance = RockModel.Instantiate<RockResource>();
            }
            else
            {
                throw new Exception($"Resource type {type} not implemented");
            }
            AddChild(instance);
            NaturalResources.Add(instance);

            instance.GlobalPosition = position;
            instance.Cell = cell;
            instance.NaturalResourceManager = this;

            OccupiedCells[cell.X, cell.Y] = true;
            return true;
        }
    }
}
