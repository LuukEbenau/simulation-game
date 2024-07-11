using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    public partial class Worker: Unit
    {
        public override void _Ready()
        {
        }

        protected override void PerformAction()
        {
            var cell = this.GameManager.MapManager.WorldToCell(GlobalPosition);
            //TOOD: actual functionality
            this.GameManager.BuildingManager.BuildBuilding(cell, new Road(), map.BuildingRotation.Bottom);
        }


        public override void _Process(double delta)
        {
            ManagePathfinding(delta);
        }
    }
}
