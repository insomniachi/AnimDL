using System.Reflection;

namespace AnimDL.Core.Api;

public interface IPlugin
{
    void RegisterProviders(IPluginRegistrationContext registrationContext);
}

public interface IPluginRegistrationContext
{
    void RegisterPlugin<TProvider>(string displayName, string name, Guid id) where TProvider : IProvider, new();
    ProviderInfo? UnloadPlugin(string name);
    IEnumerable<ProviderInfo> Providers { get; }
    IProvider? GetProvider(string name);
}

public class PluginRegistrationContext : IPluginRegistrationContext
{
    private readonly List<ProviderInfo> _plugins = new();

    public IEnumerable<ProviderInfo> Providers => _plugins;

    public IProvider? GetProvider(string name) => _plugins.FirstOrDefault(x => x.Name == name)?.Provider.Value;

    public void RegisterPlugin<TProvider>(string displayName, string name, Guid id)
        where TProvider : IProvider, new()
    {
        _plugins.Add(new ProviderInfo
        {
            DisplayName = displayName,
            Name = name,
            Id = id,
            Provider = new Lazy<IProvider>(() => new TProvider()),
            Version = Assembly.GetCallingAssembly().GetName().Version!
        });
    }

    public ProviderInfo? UnloadPlugin(string name)
    {
        var plugin = _plugins.FirstOrDefault(p => p.Name == name);

        if(plugin is not null)
        {
            _plugins.Remove(plugin);
        }

        return plugin;
    }
}

public class ProviderInfo
{
    public ProviderInfo()
    {
    }

    required public Guid Id { get; init; }
    required public string Name { get; init; }
    required public string DisplayName { get; init; }
    required public Lazy<IProvider> Provider { get; init; }
    required public Version Version { get; init; }
}
