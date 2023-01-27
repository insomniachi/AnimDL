using System.Reflection;
using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IPlugin
{
    void RegisterProviders(IPluginRegistrationContext registrationContext);
    void SetOptions(ProviderOptions options);
    ProviderOptions GetOptions();
    string GetName() => Assembly.GetCallingAssembly().GetName().Name!;
}
