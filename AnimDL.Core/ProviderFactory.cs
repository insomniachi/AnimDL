using System.Runtime.Loader;
using AnimDL.Core.Api;
using AnimDL.Core.Models;

namespace AnimDL.Core;

public class ProviderFactory : IProviderFactory
{
    private readonly List<AssemblyLoadContext> _assemblyLoadContexts = new();
    private readonly PluginContext _pluginContext = new();
    private readonly Dictionary<string, IPlugin> _plugins = new();

    public static ProviderFactory Instance { get; } = new ProviderFactory();
    public IEnumerable<ProviderInfo> Providers => _pluginContext.Providers;
    public IProvider? GetProvider(string name) => _pluginContext.GetProvider(name);

    public IParameters GetConfiguration(string name) => _plugins[name].GetDefaultConfig();
    public void SetConfiguration(string name, IParameters configuration) => _plugins[name].Initialize(configuration);

    public void LoadPlugins(string folder)
    {
        var files = Directory.GetFiles(folder, "*.dll");
        foreach (var dll in files)
        {
            var context = new AssemblyLoadContext(Path.GetFullPath(dll), true);

            var assembly = context.LoadFromAssemblyPath(dll);
            var plugins = assembly.GetExportedTypes().Where(x => x.IsAssignableTo(typeof(IPlugin))).ToList();

            if (plugins.Count == 0)
            {
                context.Unload();
                continue;
            }

            foreach (var type in plugins)
            {
                if (!type.IsAssignableTo(typeof(IPlugin)) || !(type.FullName is { } pluginType))
                {
                    continue;
                }

                if (assembly.CreateInstance(pluginType) is not IPlugin plugIn)
                {
                    continue;
                }

                plugIn.RegisterProviders(_pluginContext);
                _plugins.Add(_pluginContext.Providers.Last().Name, plugIn);
            }
            _assemblyLoadContexts.Add(context);
        }
    }

    public void UnloadPlugins()
    {
        foreach (var item in _pluginContext.Providers)
        {
            UnloadPlugin(item.Name);
        }
    }

    public void LoadPlugin<TPlugin>()
        where TPlugin : IPlugin, new()
    {
        var plugin = new TPlugin();
        plugin.RegisterProviders(_pluginContext);
    }

    public void UnloadPlugin(string name)
    {
        if (_pluginContext.UnloadPlugin(name) is not { } plugin)
        {
            return;
        }

        if (AssemblyLoadContext.GetLoadContext(plugin.Provider.Value.GetType().Assembly) is not { } loadContext)
        {
            return;
        }

        _assemblyLoadContexts.Remove(loadContext);
        loadContext.Unload();
    }
}

