using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units
{
    internal class Worker(UnitGender gender) : Unit(gender)
    {
        public override UnitType Type { get; } = UnitType.WORKER;
    }
}
