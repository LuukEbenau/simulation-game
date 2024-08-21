using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.units.professions.misc;

namespace SacaSimulationGame.scripts.units.dataObjects
{
    [Flags]
    public enum UnitGender
    {
        MALE = 1,
        FEMALE = 2
    }
    public class UnitDataObject
    {
        public UnitDataObject(UnitGender gender, ProfessionType profession)
        {
            this.Gender = gender;
            this.Profession = profession;
        }
        public ProfessionType Profession { get; }
        public UnitGender Gender { get; }
    }
}
