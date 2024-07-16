using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.units.professions;

namespace SacaSimulationGame.scripts.units.dataObjects
{
    public class WorkerDataObject(UnitGender gender):UnitDataObject(gender)
    {
        public override ProfessionType Profession { get; } = ProfessionType.Worker;
    }
}
