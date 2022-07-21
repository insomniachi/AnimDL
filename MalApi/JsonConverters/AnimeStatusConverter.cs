using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MalApi.JsonConverters
{
    public class AnimeStatusConverter : JsonConverter<AnimeStatus>
    {
        public override AnimeStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string status = reader.GetString();

            switch (status)
            {
                case "completed": return AnimeStatus.Completed;
                case "dropped": return AnimeStatus.Dropped;
                case "on_hold": return AnimeStatus.OnHold;
                case "plan_to_watch": return AnimeStatus.PlanToWatch;
                case "watching": return AnimeStatus.Watching;
                default: return AnimeStatus.None;
            }
        }

        public override void Write(Utf8JsonWriter writer, AnimeStatus value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case AnimeStatus.Completed:
                    writer.WriteStringValue("completed");
                    break;
                case AnimeStatus.Dropped:
                    writer.WriteStringValue("dropped");
                    break;
                case AnimeStatus.OnHold:
                    writer.WriteStringValue("on_hold");
                    break;
                case AnimeStatus.PlanToWatch:
                    writer.WriteStringValue("plan_to_watch");
                    break;
                case AnimeStatus.Watching:
                    writer.WriteStringValue("watching");
                    break;
            }
        }
    }
}
