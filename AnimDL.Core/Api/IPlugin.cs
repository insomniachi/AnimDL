using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IPlugin
{
    void RegisterProviders(IPluginRegistrationContext registrationContext);
    void Initialize(IParameters parameters);
    IParameters GetDefaultConfig();
}
