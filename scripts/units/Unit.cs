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
using SacaSimulationGame.scripts.units.professions;

namespace SacaSimulationGame.scripts.units
{
    public abstract partial class Unit : Node3D
    {
        public Profession Profession { get; set; }
        public GameManager GameManager { get; set; }
        public BuildingManager BuildingManager => GameManager.BuildingManager;
        public UnitManager UnitManager => GameManager.UnitManager;
        public WorldMapManager MapManager => GameManager.MapManager;

        public UnitDataObject UnitData { get; set; }

        public readonly float speed = 5;

        protected UnitBTContext context;

        public override void _Ready()
        {
            base._Ready();
        }

        public override void _Process(double delta)
        {
            context ??= new UnitBTContext();
            context.Delta = delta;

            var status = this.Profession.BehaviourTree.Tick(context);
            if (status == BehaviourStatus.Failed)
            {
                this.Profession.BehaviourTree.Reset();
            }
    
        }
    }
}
