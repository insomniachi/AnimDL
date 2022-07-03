using AnimDL.Api;
using System.CommandLine;

namespace AnimDL.Helpers;

public class AppArguments
{
    public static readonly Argument<string> Title = new("Title", "Title of anime to search");
}

public static class AppOptions
{
    public static readonly Option<ProviderType> ProviderType = new(new[] { "-p", "--provider" }, "provider name");

    static AppOptions()
    {
        ProviderType.AddCompletions(Enum.GetNames<ProviderType>());
        ProviderType.SetDefaultValue(AnimDL.Api.ProviderType.AnimixPlay);
    }
}    
