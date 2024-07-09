using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts
{
    public static class Globals
    {
        public static readonly Dictionary<CollisionType, uint> CollisionTypeMap = new()
        {
            { CollisionType.PHYSICS, 1 },
            { CollisionType.BUILDING, 2 },
            { CollisionType.WORLDMAPPING, 10 }
        };
    }
    public enum CollisionType { 
        PHYSICS = 1,
        BUILDING = 2,
        WORLDMAPPING = 10
    }
}
