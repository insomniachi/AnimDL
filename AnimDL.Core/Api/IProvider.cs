using AnimDL.Core.Api;

namespace AnimDL.Api;

public interface IProvider
{
    ProviderType ProviderType { get; }
    IStreamProvider StreamProvider { get; }
    ICatalog Catalog { get; }
}
