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
        public BuildingDO BuildingData { get; set; }

        protected Node3D BuildingVisual { get; private set; }

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
        public bool AddBuildingProgress(float progress)
        {
            var newProgress = this.BuildingData.CurrentBuildingProgress + progress;
            if (newProgress >= this.BuildingData.TotalBuildingProgressNeeded)
            {
                this.BuildingData.CurrentBuildingProgress = this.BuildingData.TotalBuildingProgressNeeded;
                this.BuildingData.BuildingCompleted = true;
                UpdateBuildingProgress();
                return true;
            }
            else
            {
                this.BuildingData.CurrentBuildingProgress += progress;
                UpdateBuildingProgress();
                return false;
            } 
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

            if (this.BuildingData.BuildingCompleted)
            {
                // Remove custom material and restore original
                meshInstance.MaterialOverride = null;
                meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.On;
                GD.Print("Building completed, original material restored");
                return;
            }

            // Calculate the alpha (transparency) value
            float baseAlpha = 0.2f;
            float maxAlpha = 0.8f;
            float alpha = this.BuildingData.BuildingCompleted ? 1.0f : baseAlpha + (maxAlpha - baseAlpha) * BuildingPercentageComplete;

            // Get or create the material
            StandardMaterial3D material = meshInstance.MaterialOverride as StandardMaterial3D;
            if (material == null)
            {
                material = new StandardMaterial3D();
                meshInstance.MaterialOverride = material;
            }

            // Set up transparency
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.AlbedoColor = new Color(1, 1, 1, alpha);

            // Ensure proper rendering of transparent objects
            material.RenderPriority = 1;
            meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

            //GD.Print($"Updated material transparency: alpha = {alpha}");
        }

        public float BuildingPercentageComplete => this.BuildingData.CurrentBuildingProgress == 0 ? 0 : this.BuildingData.CurrentBuildingProgress / this.BuildingData.TotalBuildingProgressNeeded;
        #endregion
    }
}
