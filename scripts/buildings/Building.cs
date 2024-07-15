using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings.dataObjects;


namespace SacaSimulationGame.scripts.buildings
{
    public abstract partial class Building: Node3D
    {
        public BuildingBlueprintBase Blueprint { get; set; }

        protected Node3D BuildingVisual { get; private set; }

        public abstract int MaxBuilders { get; }

        public abstract double TotalBuildingProgressNeeded { get; }
        public bool BuildingCompleted { get; set; } = false;
        private float _currentBuildingProgress = 0;
        public double CurrentBuildingProgress { get; set; }

        public Vector2I Cell { get; set; }

        public override void _Ready()
        {
            base._Ready();

            this.BuildingVisual = GetChild<Node3D>(0);
            if(this.BuildingVisual == null)
            {
                GD.Print($"WARNING: building node not found. Make sure the building scene contains a child node of type Node3D");
                GD.Print($"child node is of type {GetChild(0)}");
                PrintTreePretty();
            }

            AddBuildingProgress(0);
        }

        #region progress tracking
        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        /// <returns>finished or not</returns>
        public bool AddBuildingProgress(double progress)
        {
            var newProgress = this.CurrentBuildingProgress + progress;
            if (newProgress >= this.TotalBuildingProgressNeeded)
            {
                this.CurrentBuildingProgress = this.TotalBuildingProgressNeeded;
                this.BuildingCompleted = true;
                UpdateBuildingProgress();
                return true;
            }
            else
            {
                this.CurrentBuildingProgress += progress;
                UpdateBuildingProgress();
                return false;
            } 
        }

        /// <summary>
        /// Forcefully completes building
        /// </summary>
        public void CompleteBuilding()
        {
            this.CurrentBuildingProgress = this.TotalBuildingProgressNeeded;
            this.BuildingCompleted = true;
            UpdateBuildingProgress();
        }

        private void UpdateBuildingProgress()
        {
            if (BuildingVisual == null) return;

            // Find all MeshInstance3D children
            //var meshInstances = BuildingVisual.FindChildren("*", "MeshInstance3D", true, false).OfType<MeshInstance3D>().ToList();

            //if (meshInstances.Count == 0)
            //{
            //    GD.Print("No MeshInstance3D found in the building hierarchy");
            //    return;
            //}

            //if (this.BuildingCompleted)
            //{
            //    foreach (var meshInstance in meshInstances)
            //    {
            //        SetMeshInstanceTransparency(meshInstance, 1.0f);
            //    }
            //    GD.Print("Building completed, original material restored");
            //    return;
            //}

            //// Calculate the alpha (transparency) value
            //float baseAlpha = 0.1f;
            //float maxAlpha = 0.8f;
            //float alpha = (float)(baseAlpha + (maxAlpha - baseAlpha) * BuildingPercentageComplete);

            //foreach (var meshInstance in meshInstances)
            //{
            //    SetMeshInstanceTransparency(meshInstance, alpha);
            //}
        }

        private void SetMeshInstanceTransparency(MeshInstance3D meshInstance, float alpha)
        {
            // Get the current material
            Material material = meshInstance.GetActiveMaterial(0);

            if (material is StandardMaterial3D standardMaterial)
            {
                // If it's already a StandardMaterial3D, just modify it
                standardMaterial.AlbedoColor = new Color(
                    standardMaterial.AlbedoColor.R,
                    standardMaterial.AlbedoColor.G,
                    standardMaterial.AlbedoColor.B,
                    alpha
                );
                standardMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            }
            else
            {
                // If it's not a StandardMaterial3D, create a new one based on the existing material
                StandardMaterial3D newMaterial = new StandardMaterial3D();
                newMaterial.AlbedoColor = new Color(1, 1, 1, alpha);
                newMaterial.AlbedoTexture = (material as BaseMaterial3D)?.AlbedoTexture;
                newMaterial.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;

                meshInstance.MaterialOverride = newMaterial;
            }
        }

        public double BuildingPercentageComplete => this.CurrentBuildingProgress == 0 ? 0 : this.CurrentBuildingProgress / this.TotalBuildingProgressNeeded;
        #endregion
    }
}
