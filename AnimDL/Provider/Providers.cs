using AnimDL.Api;
using AnimDL.Core.Catalog;
using AnimDL.Core.StreamProviders;

namespace AnimDL.Provider;

public class AnimixPlayProvider : BaseProvider
{
    public AnimixPlayProvider(AnimixPlayStreamProvider provider, AnimixPlayCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimixPlay;
}

public class AnimePaheProvider : BaseProvider
{
    public AnimePaheProvider(AnimePaheStreamProvider provider, AnimePaheCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimePahe;
}

public class TenshiMoeProvider : BaseProvider
{
    public TenshiMoeProvider(TenshiMoeStreamProvider provider, TenshiMoeCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.Tenshi;
}

public class AnimeOutProvider : BaseProvider
{
    public AnimeOutProvider(AnimeOutStreamProvider provider, AnimeOutCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.AnimeOut;
}

public class GogoAnimeProvider : BaseProvider
{
    public GogoAnimeProvider(GogoAnimeStreamProvider provider, GogoAnimeCatalog catalog) : base(provider, catalog) { }
    public override ProviderType ProviderType => ProviderType.GogoAnime;
}
