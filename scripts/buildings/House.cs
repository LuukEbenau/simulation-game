using Godot;
using SacaSimulationGame.scripts.buildings;
using System;
namespace SacaSimulationGame.scripts.buildings
{
    public partial class House : Building
    {
        public override int MaxBuilders => 1;
        public override double TotalBuildingProgressNeeded => 15;
       

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);
        }
    }

}