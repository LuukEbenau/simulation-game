using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Godot;

namespace SacaSimulationGame.scripts.map
{
    public class Vector2IConverter : JsonConverter<Vector2I>
    {
        public override Vector2I ReadJson(JsonReader reader, Type objectType, Vector2I existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            s = s.Trim('(', ')');
            var parts = s.Split(',');
            return new Vector2I(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        public override void WriteJson(JsonWriter writer, Vector2I value, JsonSerializer serializer)
        {
            writer.WriteValue($"({value.X},{value.Y})");
        }
    }
}
