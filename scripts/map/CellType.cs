using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.map
{
    [Flags]
    public enum CellType
    {
        NONE = 0,
        GROUND = 1,
        WATER =2,
        HILL = 4
    }
}
