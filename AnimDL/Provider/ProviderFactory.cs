using AnimDL.Api;

namespace AnimDL.Provider;

public class ProviderFactory : IProviderFactory
{
    private readonly IEnumerable<IProvider> _providers;

    public ProviderFactory(IEnumerable<IProvider> providers)
    {
        _providers = providers;
    }

    public IProvider GetProvider(ProviderType type) => _providers.First(x => x.ProviderType == type);
}
