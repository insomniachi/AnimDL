using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IMalCatalog
{
    Task<SearchResult> SearchByMalId(long id);
}