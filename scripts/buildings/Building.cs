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
        [Export] public PackedScene ModelCompleted { get; set; }
        [Export] public PackedScene ModelConstruction { get; set; }

        public BuildingBlueprintBase Blueprint { get; set; }
        public double BuildingPercentageComplete => this.CurrentBuildingProgress == 0 ? 0 : this.CurrentBuildingProgress / this.TotalBuildingProgressNeeded;
        protected Node3D BuildingVisual { get; set; }

        public abstract int MaxBuilders { get; }

        public BuildingResources ResourcesRequiredForBuilding { get; protected set; }

        public abstract double TotalBuildingProgressNeeded { get; }
        public bool BuildingCompleted { get; set; } = false;
        private float _currentBuildingProgress = 0;
        public double CurrentBuildingProgress { get; set; }

        public Vector2I Cell { get; set; }

        public abstract BuildingType Type { get; }
        public abstract bool IsResourceStorage { get;  }
        public void RotateBuilding(BuildingRotation rotation)
        {
            Quaternion rot = new Quaternion(
                new Vector3(0, 1, 0), 
                Mathf.DegToRad(-(float)rotation)
            );
            this.VisualWrap.Basis = new Basis(rot);
            //this.VisualWrap.RotationDegrees = new Vector3(0, -(float)rotation, 0);
        }

        

        /// <summary>
        /// Element that wraps the visual
        /// </summary>
        protected Node3D VisualWrap => _visualWrap ??= GetChild<Node3D>(0);
        private Node3D _visualWrap;

        public override void _Ready()
        {
            base._Ready();
            //VisualWrap = GetChild<Node3D>(0);

            UpdateBuildingProgress();

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
        /// Whether buildings can continue working or have to wait for resources
        /// </summary>
        /// <returns></returns>
        public bool CanBuild()
        {
            var percentageBuildingComplete =  this.CurrentBuildingProgress / this.TotalBuildingProgressNeeded;
            if(percentageBuildingComplete <= ResourcesRequiredForBuilding.PercentageResourcesAquired)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

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
            //UpdateBuildingProgress();
        }

        //protected abstract void UpdateBuildingProgress();
        private PackedScene __lastShownVisual = null;
        protected void UpdateBuildingProgress()
        {
            PackedScene visual;
            if (!this.BuildingCompleted)
            {
                visual = this.ModelConstruction;
            }
            else
            {
                visual = this.ModelCompleted;
            }

            if (visual != this.__lastShownVisual)
            {
                this.__lastShownVisual = visual;

                if (BuildingVisual != null) VisualWrap.RemoveChild(BuildingVisual);
                BuildingVisual = visual.Instantiate<Node3D>();

                VisualWrap.AddChild(BuildingVisual);
            }
        }

        
        #endregion
    }
}
