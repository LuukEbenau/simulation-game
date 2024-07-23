using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.naturalResources
{
    [Flags]
    public enum ResourceType
    {
        AllResources = Wood | Stone | Fish,
        StockpileResources = Wood | Stone,
        //None = 0,
        Wood = 1,
        Stone = 2,
        Fish = 4,

    }
}
