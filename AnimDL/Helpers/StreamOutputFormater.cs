using AnimDL.Api;
using AnimDL.Core.Models;
using System.Text.Json;

namespace AnimDL.Helpers
{
    public static class StreamOutputFormater
    {
        public static string Format(this VideoStreamsForEpisode streamsForEp, ProviderType type)
        {
            return type switch
            {
                ProviderType.AnimixPlay => AnimixPlayFormat(streamsForEp),
                _ => JsonSerializer.Serialize(streamsForEp)
            };
        }

        private static string AnimixPlayFormat(VideoStreamsForEpisode streamsForEp)
        {
            return $"[Ep - {streamsForEp.Episode}] => {streamsForEp.Qualities.FirstOrDefault().Value.Url}";
        }
    }
}
