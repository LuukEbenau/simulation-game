using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.managers
{
    public partial class NaturalResourceManager: Node3D
    {
        [Export] public PackedScene TreeModel { get; set; }

        public List<NaturalResource> NaturalResources { get; set; } = [];

        public GameManager GameManager { get; set; }

        public NaturalResourceManager() { 
        
        }

        public override void _Ready()
        {
            base._Ready();
            GameManager = GetParent<GameManager>();
        }

        public bool AddResource(Vector3 position, NaturalResourceType type) {
            if (type == NaturalResourceType.Tree) {
                var instance = TreeModel.Instantiate<TreeResource>();

                AddChild(instance);

                instance.GlobalPosition = position;
                //TODO: something with the cells, so that we can prevent multiple resources on same cell, or resources on top of buildings, etc.
                NaturalResources.Add(instance);
                return true;
            }
            else
            {
                throw new Exception($"Resource type {type} not implemented");
            }
            //NaturalResources
        }
    }
}
