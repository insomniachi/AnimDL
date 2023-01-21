using AnimDL.Core.Api;
using AnimDL.Core.Models;

namespace Plugin.KamyRoll;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("4E565978-1E7C-4176-884A-F821DF1E102F");

    public IParameters GetDefaultConfig()
    {
        return new Parameters()
        {
            [nameof(Config.Channel)] = Config.Channel,
            [nameof(Config.Locale)] = Config.Locale,
            [nameof(Config.AccessToken)] = Config.AccessToken,
            [nameof(Config.SearchLimit)] = Config.SearchLimit,
            [nameof(Config.StreamType)] = Config.StreamType,
        };
    }

    public void Initialize(IParameters parameters)
    {
        if(parameters.TryGetValue(nameof(Config.Channel), out string channel))
        {
            Config.Channel = channel;
        }
        if (parameters.TryGetValue(nameof(Config.Locale), out string locale))
        {
            Config.Locale = locale;
        }
        if (parameters.TryGetValue(nameof(Config.AccessToken), out string token))
        {
            Config.AccessToken = token;
            if(!string.IsNullOrEmpty(token))
            {
                KamyRollClient.SetAccessToken();
            }
        }
        if (parameters.TryGetValue(nameof(Config.SearchLimit), out string limit))
        {
            Config.SearchLimit = limit;
        }
    }

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<KamyRollClient>("Kamy Roll", "kamy", Id);
    }
}

public static class Config
{
    public static string Channel { get; set; } = "crunchyroll";
    public static string Locale { get; set; } = "en-US";
    public static string AccessToken { get; set; } = string.Empty;
    public static string SearchLimit { get; set; } = "5";
    public static string StreamType { get; set; } = "sub";
}
