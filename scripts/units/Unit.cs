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
        public GameManager GameManager { get; set; }
        public UnitDataObject UnitData { get; set; }
    }
}
