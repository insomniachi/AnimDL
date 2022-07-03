using AnimDL.Api;
using AnimDL.Core.Api;

namespace AnimDL.Provider;

public abstract class BaseProvider : IProvider
{
    public abstract ProviderType ProviderType { get; }
    public IStreamProvider StreamProvider { get; }
    public ICatalog Catalog { get; }

    public BaseProvider(IStreamProvider provider, ICatalog catalog)
    {
        StreamProvider = provider;
        Catalog = catalog;
    }
}
