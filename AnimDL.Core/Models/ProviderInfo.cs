using AnimDL.Core.Api;

namespace AnimDL.Core.Models;

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
