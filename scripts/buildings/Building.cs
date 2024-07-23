using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.managers;


namespace SacaSimulationGame.scripts.buildings
{
    public abstract partial class Building: Node3D
    {
        [Export] public PackedScene ModelCompleted { get; set; }
        [Export] public PackedScene ModelConstruction { get; set; }

        protected Node3D BuildingVisual { get; set; }

        public BuildingBlueprintBase Blueprint { get; set; }

        public GameManager GameManager { get; set; }

        public abstract int MaxBuilders { get; }
        public abstract BuildingType Type { get; }
        public abstract bool IsResourceStorage { get; }

        [Signal]
        public delegate void OnBuildingCompletedEventHandler();

        public readonly int MaxNumberOfEmployees = 1;
        public List<Unit> WorkingEmployees { get; set; } = [];


        /// <summary>
        /// The cell where the building is located
        /// </summary>
        public Vector2I Cell { get; set; }

        // Building of the building
        public double BuildingPercentageComplete => this.CurrentBuildingProgress == 0 ? 0 : this.CurrentBuildingProgress / this.TotalBuildingProgressNeeded;
        public abstract double TotalBuildingProgressNeeded { get; }
        public bool BuildingCompleted { get; set; } = false;
        private float _currentBuildingProgress = 0;
        public double CurrentBuildingProgress { get; set; }
        public BuildingResources BuildingResources { get; protected set; }

        public void RotateBuilding(BuildingRotation rotation)
        {
            Quaternion rot = new(
                new Vector3(0, 1, 0), 
                Mathf.DegToRad(-(float)rotation)
            );

            this.VisualWrap.Basis = new Basis(rot);
        }

        /// <summary>
        /// Element that wraps the visual
        /// </summary>
        protected Node3D VisualWrap { get; private set; }

        public override void _Ready()
        {
            base._Ready();

            this.VisualWrap = GetNode<Node3D>("VisualWrap");
            this.BuildingResources = GetNode<BuildingResources>("BuildingResources");

            OnBuildingCompleted += Building_OnBuildingCompleted;

            UpdateBuildingProgress();

            if(this.BuildingVisual == null)
            {
                GD.Print($"WARNING: building node not found. Make sure the building scene contains a child node of type Node3D");
                GD.Print($"child node is of type {GetChild(0)}");
                PrintTreePretty();
            }

            AddBuildingProgress(0);
        }

        private void Building_OnBuildingCompleted()
        {
            this.BuildingResources.ShowResources(false);
        }

        #region progress tracking

        /// <summary>
        /// Whether buildings can continue working or have to wait for resources
        /// </summary>
        /// <returns></returns>
        public bool CanBuild()
        {
            var percentageBuildingComplete =  this.CurrentBuildingProgress / this.TotalBuildingProgressNeeded;
            if(percentageBuildingComplete <= BuildingResources.PercentageResourcesAquired)
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

                //GD.Print("building completed");
                UpdateBuildingProgress();

                this.EmitSignal(SignalName.OnBuildingCompleted);
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
            this.EmitSignal(SignalName.OnBuildingCompleted);
        }

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

                if (BuildingVisual != null){ 
                    foreach (var child in VisualWrap.GetChildren())
                    {
                        VisualWrap.RemoveChild(child);
                        child.QueueFree();
                    }
                }
                BuildingVisual = visual.Instantiate<Node3D>();

                VisualWrap.AddChild(BuildingVisual);
            }
        }
      
        #endregion
    }
}
