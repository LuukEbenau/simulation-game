using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.units.professions;

namespace SacaSimulationGame.scripts.units.dataObjects
{
    [Flags]
    public enum UnitGender
    {
        MALE = 1,
        FEMALE = 2
    }
    public abstract class UnitDataObject(UnitGender gender)
    {
        public abstract ProfessionType Profession { get; }
        public UnitGender Gender { get; } = gender;
    }
}
