using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class Bridge : BuildingBase
    {
        public override int MaxBuilders => 1;

        public override BuildingType Type => BuildingType.Bridge;

        public override bool IsResourceStorage => false;

        public override double TotalBuildingProgressNeeded => 10;
    }
}
