using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units
{
    public class Builder(UnitGender gender) : Unit(gender) 
    {
        public override UnitType Type { get; } = UnitType.BUILDER;
    }
}
