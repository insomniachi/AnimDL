using AnimDL.Core.Api;

namespace AnimDL.Core;

public abstract class BaseProvider : IProvider
{
    public IStreamProvider StreamProvider { get; }
    public ICatalog Catalog { get; }
    public IAiredEpisodeProvider? AiredEpisodesProvider { get; }

    public BaseProvider(IStreamProvider provider, ICatalog catalog, IAiredEpisodeProvider? airedEpisodesProvider = null)
    {
        StreamProvider = provider;
        Catalog = catalog;
        AiredEpisodesProvider = airedEpisodesProvider;
    }
}

