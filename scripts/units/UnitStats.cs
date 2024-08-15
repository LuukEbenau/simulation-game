using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units
{
    public class UnitStats
    {
        public float Health { get; set; } = MAXHEALTH;
        public const float MAXHEALTH = 100;
        //public float MaxHealth => 100;
        public float Speed { get; set; }
        //public string Name { get; set; }

        //private static int _unitIdx = 0;
        public UnitStats() {
            //this.Name = "Wild Beaver " + _unitIdx++;
        }
    }
}
