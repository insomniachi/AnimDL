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
    public static readonly Option<int> Episode = new(new[] { "-ep", "--episode" }, "episode number (starts from 1)");

    static AppOptions()
    {
        Episode.SetDefaultValue(1);
        ProviderType.AddCompletions(Enum.GetNames<ProviderType>());
    }
}    
