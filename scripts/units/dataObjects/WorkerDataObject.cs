using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.units.dataObjects
{
    public class WorkerDataObject(UnitGender gender):UnitDataObject(gender)
    {
        public override UnitType Type { get; } = UnitType.WORKER;
    }
}
