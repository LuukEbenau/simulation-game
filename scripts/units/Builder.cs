using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.units.dataObjects;


namespace SacaSimulationGame.scripts.units
{
    public partial class Builder : Unit
    {
        public override void _Ready()
        {
            GD.Print("builder ready");
        }

        public override void _Process(double delta)
        {
            ManagePathfinding(delta);
        }

        protected override void PerformAction()
        {
            try
            {
                var cell = this.GameManager.MapManager.WorldToCell(GlobalPosition);
                //TOOD: actual functionality
                this.GameManager.BuildingManager.BuildBuilding(cell, new HouseDO{ Rotation = map.BuildingRotation.Bottom});
            }
            catch (Exception ex)
            {
                GD.Print($"Error happend trying to build a building for {Name}, error: {ex.Message}");
            }
        }
    }
}
