﻿using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IProviderFactory
{
    IEnumerable<ProviderInfo> Providers { get; }
    IProvider? GetProvider(string name);
    void LoadPlugins(string folder);
    void LoadPlugin<TPlugin>() where TPlugin : IPlugin, new();
    void UnloadPlugin(string name);
    void UnloadPlugins();
    ProviderOptions GetOptions(string providerName);
    bool SetOptions(string name, ProviderOptions options);
}
