using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    public partial class Unit: Node3D
    {
        private Random _rnd;

        public GameManager GameManager { get; set; }
        public UnitDataObject UnitData { get; set; }

        protected Vector2I GetNewDestination()
        {
            Vector3 destination = new(_rnd.Next(-40, 40), 0, _rnd.Next(-40, 40));

            var destinationCell = GameManager.MapManager.WorldToCell(destination);
            return destinationCell;
        }

        public Unit()
        {
            _rnd = new Random();
        }
    }
}
