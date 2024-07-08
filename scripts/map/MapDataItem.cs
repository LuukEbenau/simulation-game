using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameTemplate.scripts.map
{
    public class MapDataItem
    {
        public float Slope { get; set; }

        public override string ToString()
        {
            return "{\"Slope\":"+Slope+"}";
        }
    }
}
