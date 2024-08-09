using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;
using Windows.Services.Maps;

namespace SacaSimulationGame.scripts.managers
{
    public partial class NaturalResourceManager: Node3D
    {
        [Export] public PackedScene TreeModel { get; set; }

        public List<NaturalResource> NaturalResources { get; set; } = [];

        public GameManager GameManager { get; set; }

        private bool[,] _occupiedCells;
        public bool[,] OccupiedCells  => _occupiedCells ??= new bool[GameManager.MapManager.MapWidth, GameManager.MapManager.MapHeight]; 

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

        public bool AddResource(Vector3 position, NaturalResourceType type) {
            var cell = this.GameManager.MapManager.WorldToCell(position);

            var cellOccupation = GameManager.MapManager.GetCellOccupation(cell);
            if (cellOccupation.IsOccupied) {
                return false;
            }

            if (type == NaturalResourceType.Tree) {
                var instance = TreeModel.Instantiate<TreeResource>();

                AddChild(instance);
                NaturalResources.Add(instance);
   
                instance.GlobalPosition = position;
                instance.Cell = cell;
                instance.NaturalResourceManager = this;

                OccupiedCells[cell.X, cell.Y] = true;
                return true;
            }
            else
            {
                throw new Exception($"Resource type {type} not implemented");
            }
        }
    }
}
