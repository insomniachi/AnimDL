using System.Reflection;
using AnimDL.Core.Api;
using AnimDL.Core.Models;

namespace AnimDL.Core;

public class PluginContext : IPluginRegistrationContext, IPluginResolver
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

        if (plugin is not null)
        {
            _plugins.Remove(plugin);
        }

        return plugin;
    }
}
