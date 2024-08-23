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

            var rand = new Random();
            var resourceAmount = rand.Next(3, 11) * 10;
            this.ResourceStorage.MaxCapacity = resourceAmount;
            this.ResourceStorage.AddResource(ResourceType.Wood, resourceAmount);
            //this.ResourceStorage.CurrentCapacity = resourceAmount;

            //Scale *= 0.3f; // to make default size a bit smaller
            TreeModel = ResourceLoader.Load<PackedScene>("res://assets/naturalResources/models/tree/tree.blend");
            Scale *= resourceAmount / 100f;

            // random rotation
            float rotation = rand.Next(0, 360);
            this.RotationDegrees += new Vector3(0, rotation, 0);

            this.ResourceStorage.StoredResourcesChanged += UpdateResourceVisual;
            UpdateResourceVisual();
        }

        PackedScene CurrentResourceVisualScene;
        Node3D CurrentModel;

        private void UpdateResourceVisual()
        {
            Node3D model = null;
            if (this.ResourceStorage.GetStorageCapacityLeft(ResourceType.Wood) == 0)
            {
                if (CurrentResourceVisualScene == null || CurrentResourceVisualScene != TreeModel)
                {
                    CurrentResourceVisualScene = TreeModel;
                    model = TreeModel.Instantiate<Node3D>();
                }
            }
            else if (this.ResourceStorage.CurrentCapacity == 0) {
                // resource is empty, and has to be removed
                this.NaturalResourceManager.RemoveResource(this);
            }
            else
            {
                if (CurrentResourceVisualScene == null || CurrentResourceVisualScene != TreeModelFelled)
                {
                    CurrentResourceVisualScene = TreeModelFelled;
                    model = TreeModelFelled.Instantiate<Node3D>();
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
