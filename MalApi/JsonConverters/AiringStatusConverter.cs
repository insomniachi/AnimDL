using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class AiringStatusConverter : JsonConverter<AiringStatus>
    {
        public override AiringStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            switch (text)
            {
                case "finished_airing": return AiringStatus.FinishedAiring;
                case "currently_airing": return AiringStatus.CurrentlyAiring;
                case "not_yet_aired": return AiringStatus.NotYetAired;
                default: return AiringStatus.NotYetAired;
            }
        }

        public override void Write(Utf8JsonWriter writer, AiringStatus value, JsonSerializerOptions options)
        {
            switch(value)
            {
                case AiringStatus.FinishedAiring:
                    writer.WriteStringValue("finished_airing");
                    break;
                case AiringStatus.CurrentlyAiring:
                    writer.WriteStringValue("currently_airing");
                    break;
                case AiringStatus.NotYetAired:
                    writer.WriteStringValue("not_yet_aired");
                    break;
            }
        }
    }
}
