using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IPluginRegistrationContext
{
    void RegisterPlugin<TProvider>(string displayName, string name, Guid id) where TProvider : IProvider, new();
    ProviderInfo? UnloadPlugin(string name);
    IEnumerable<ProviderInfo> Providers { get; }
    IProvider? GetProvider(string name);
}
