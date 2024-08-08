using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.helpers
{
    public static class EnumHelpers
    {
        private static readonly ResourceType[] resourcesToCheck = { ResourceType.Wood, ResourceType.Stone, ResourceType.Fish };
        public static IEnumerable<ResourceType> GetActiveFlags(this ResourceType flags)
        {
            
            foreach(var r in resourcesToCheck)
            {
                if (flags.HasFlag(r)) yield return r;
            }
            
        }
    }
}
