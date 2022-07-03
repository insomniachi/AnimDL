using AnimDL.Api;
using AnimDL.Core.Models;
using System.Text.Json;

namespace AnimDL.Helpers
{
    public static class StreamOutputFormater
    {
        public static string Format(this HlsStreams streams, ProviderType type)
        {
            return type switch
            {
                ProviderType.AnimixPlay => FormatAnimixPlay(streams),
                _ => JsonSerializer.Serialize(streams)
            };
        }

        private static string FormatAnimixPlay(HlsStreams streams)
        {
            return $"[Ep - {streams.episode}] => {streams.streams[0].stream_url}";
        }
    }
}
