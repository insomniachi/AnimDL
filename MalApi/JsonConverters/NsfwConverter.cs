using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class NsfwConverter : JsonConverter<NsfwLevel>
    {
        public override NsfwLevel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            switch(text)
            {
                case "white": return NsfwLevel.White;
                case "gray": return NsfwLevel.Gray;
                case "black": return NsfwLevel.Black;
                default: return NsfwLevel.White;
            }
        }

        public override void Write(Utf8JsonWriter writer, NsfwLevel value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString().ToLower());
        }
    }
}
