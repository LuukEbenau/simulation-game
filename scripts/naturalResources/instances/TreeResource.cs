using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace SacaSimulationGame.scripts.naturalResources
{
    public partial class TreeResource: NaturalResource
    {
        [Export] public PackedScene TreeModel { get; set; }
        [Export] public PackedScene TreeModelFelled { get; set; }

        public override void _Process(double delta)
        {
            base._Process(delta);
        }

        public override void _Ready()
        {
            base._Ready();

            this.ResourceStorage.StoredResourcesChanged += UpdateResourceVisual;
            UpdateResourceVisual();
        }

        PackedScene CurrentResourceVisualScene;
        Node3D CurrentModel;

        private void UpdateResourceVisual()
        {
            Node3D model = null; 
            if(this.ResourceStorage.GetStorageSpaceLeft(ResourceType.Wood) > 0)
            {
                if (CurrentResourceVisualScene == null || CurrentResourceVisualScene != TreeModelFelled)
                {
                    CurrentResourceVisualScene = TreeModelFelled;
                    model = TreeModelFelled.Instantiate<Node3D>();
                }
            }
            else
            {
                if (CurrentResourceVisualScene == null || CurrentResourceVisualScene != TreeModel)
                {
                    CurrentResourceVisualScene = TreeModel;
                    model = TreeModel.Instantiate<Node3D>();
                }
            }

            if (model != null)
            {
                if (CurrentModel != null) VisualWrap.RemoveChild(CurrentModel);
                VisualWrap.AddChild(model);
                CurrentModel = model;
            }
        }
    }
}
