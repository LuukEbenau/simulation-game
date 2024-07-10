using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units
{
    [Flags]
    public enum UnitGender
    {
        MALE = 1,
        FEMALE = 2
    }
    public abstract class Unit(UnitGender gender)
    {
        public abstract UnitType Type { get; }
        public UnitGender Gender { get; } = gender;
    }
}
