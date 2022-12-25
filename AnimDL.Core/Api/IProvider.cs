namespace AnimDL.Core.Api;

public interface IProvider
{
    ProviderType ProviderType { get; }
    IStreamProvider StreamProvider { get; }
    ICatalog Catalog { get; }
    IAiredEpisodeProvider? AiredEpisodesProvider { get; }
}
