namespace AnimDL.Core.Api;

public interface IProviderFactory
{
    IEnumerable<ProviderInfo> Providers { get; }
    IProvider? GetProvider(string name);
    void LoadPlugins(string folder);
    void LoadPlugin<TPlugin>() where TPlugin : IPlugin, new();
    void UnloadPlugin(string name);
}
