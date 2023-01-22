using AnimDL.Core.Api;

namespace AnimDL.Core.Tests;

internal class Helper
{
    internal static IProvider GetProvider(string name)
    {
        return name switch
        {
            "allanime" => new Plugin.AllAnime.Provider(),
            "animepahe" => new Plugin.AnimePahe.Provider(),
            "marin" => new Plugin.Marin.Provider(),
            "yugen" => new Plugin.Yugen.Provider(),
            "gogo" => new Plugin.GogoAnime.Provider(),
            "kamy" => new Plugin.KamyRoll.KamyRollClient(),
            "consumet" => new Plugin.Consumet.Provider(),
            _ => throw new NotSupportedException()
        };
    }
}
