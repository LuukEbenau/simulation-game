using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace GameTemplate.scripts.map
{
    public enum CellType { 
        NONE,
        GROUND,
        WATER
    }
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
