using Godot;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions;
using System;
namespace SacaSimulationGame.scripts.managers
{
    public partial class UnitManager : Node3D, IUnitManager
    {
        [ExportCategory("Unit Scenes")]
        [Export]
        public PackedScene UnitScene { get; set; }


        private GameManager GameManager { get; set; }
        // Called when the node enters the scene tree for the first time.
        //private 
        public override void _Ready()
        {
            this.GameManager = this.GetParent<GameManager>();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }

        public bool SpawnUnit(Vector3 spawnLocation, UnitDataObject unit)
        {
            Unit instance = this.UnitScene.Instantiate<Unit>();

            instance.ChangeProfession(unit.Profession);

            //instance.UnitData = unit;
            instance.GameManager = this.GameManager;

            AddChild(instance);
            instance.GlobalPosition = spawnLocation;

            return true;
        }
    }
}
