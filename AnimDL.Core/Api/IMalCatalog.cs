using AnimDL.Api;
using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IMalCatalog
{
    Task<(SearchResult Sub, SearchResult? Dub)> SearchByMalId(long id);
}