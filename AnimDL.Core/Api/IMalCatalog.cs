using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface IMalCatalog
{
    Task<(SearchResult Sub, SearchResult? Dub)> SearchByMalId(long id);
}

public interface ICanParseMalId
{
    Task<long> GetMalId(string url);
}