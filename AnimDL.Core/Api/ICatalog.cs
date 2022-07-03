using AnimDL.Core.Models;

namespace AnimDL.Core.Api;

public interface ICatalog
{
    IAsyncEnumerable<SearchResult> Search(string query);
}