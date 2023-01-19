using AnimDL.Core.Api;

namespace Plugin.KamyRoll;

public class Plugin : IPlugin
{
    static Guid Id { get; } = Guid.Parse("4E565978-1E7C-4176-884A-F821DF1E102F");

    public void RegisterProviders(IPluginRegistrationContext registrationContext)
    {
        registrationContext.RegisterPlugin<KamyRollClient>("Kamy Roll", "kamy", Id);
    }
}
