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

            // Find the first MeshInstance3D child
            MeshInstance3D meshInstance = BuildingVisual.FindChild("*", true, false) as MeshInstance3D;
            if (meshInstance == null)
            {
                GD.Print("No MeshInstance3D found in the building hierarchy");
                return;
            }

            if (this.BuildingCompleted)
            {
                // Remove custom material and restore original
                GD.Print("Building completed, original material restored");
                meshInstance.Transparency = 0;
                return;
            }

            // Calculate the alpha (transparency) value
            float baseAlpha = 0.1f;
            float maxAlpha = 0.8f;
            float alpha = (float)(this.BuildingCompleted ? 1.0f : baseAlpha + (maxAlpha - baseAlpha) * BuildingPercentageComplete);

            // Get or create the material

            meshInstance.Transparency = 1-alpha;

            //StandardMaterial3D material = meshInstance.MaterialOverride as StandardMaterial3D;
            //if (material == null)
            //{
            //    material = new StandardMaterial3D();
            //    meshInstance.MaterialOverride = material;
            //}

            //// Set up transparency
            //material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            //material.AlbedoColor = new Color(1, 1, 1, alpha);

            //// Ensure proper rendering of transparent objects
            //material.RenderPriority = 1;
            //meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            //GD.Print($"Updated material transparency: alpha = {alpha}");
        }

        public double BuildingPercentageComplete => this.CurrentBuildingProgress == 0 ? 0 : this.CurrentBuildingProgress / this.TotalBuildingProgressNeeded;
        #endregion
    }
}
