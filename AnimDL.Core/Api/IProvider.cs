using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IProvider
{
    IStreamProvider StreamProvider { get; }
    ICatalog Catalog { get; }
    IAiredEpisodeProvider? AiredEpisodesProvider { get; }
}
