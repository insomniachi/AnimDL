using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IAiredEpisodeProvider
{
    Task<IEnumerable<AiredEpisode>> GetRecentlyAiredEpisodes(int page = 1);
}
