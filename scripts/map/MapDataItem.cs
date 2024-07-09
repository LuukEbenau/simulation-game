using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.map;
namespace SacaSimulationGame.scripts.map
{

    public class MapDataItem
    {
        public CellType CellType;

        public float Slope { get; set; }
        public float Height { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
