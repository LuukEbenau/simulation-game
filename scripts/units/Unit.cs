using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    public abstract partial class Unit : Node3D
    {
        /// <summary>
        /// The current task of the Unit
        /// </summary>
        //public UnitTask Task { get; protected set; }


        public GameManager GameManager { get; set; }
        public BuildingManager BuildingManager => GameManager.BuildingManager;
        public UnitManager UnitManager => GameManager.UnitManager;
        public WorldMapManager MapManager => GameManager.MapManager;

        public UnitDataObject UnitData { get; set; }

        private readonly float speed = 5;

        protected UnitBTContext context;

        protected IBehaviour<UnitBTContext> BehaviourTree { get; set; }

        public override void _Ready()
        {
            base._Ready();
            this.BehaviourTree = GetBehaviourTree();
        }

        public override void _Process(double delta)
        {
            context ??= new UnitBTContext();
            context.Delta = delta;

            var status = this.BehaviourTree.Tick(context);
            if (status == BehaviourStatus.Failed)
            {
                this.BehaviourTree.Reset();
            }
    
        }
    }
}
