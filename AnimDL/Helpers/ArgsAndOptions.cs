using AnimDL.Api;
using System.CommandLine;
using System.Text.RegularExpressions;

namespace AnimDL.Helpers;

public class AppArguments
{
    public static readonly Argument<string> Title = new("Title", "Title of anime to search");
}

public static class AppOptions
{
    public static readonly Option<ProviderType> ProviderType = new(new[] { "-p", "--provider" }, "provider name");
    public static readonly Option<MediaPlayerType> MediaPlayer = new(new[] { "--player" }, () => MediaPlayerType.Vlc, "media player to stream.");
    public static readonly Option<Range> Range = new(aliases: new[] { "-r", "--range" }, description: "range of episodes", parseArgument: x =>
    {
        var str = x.Tokens[0].Value;
        var match = Regex.Match(str, @"(?'startFromEnd'\^)?(?'start'\d+)(?'isRange'..)?(?'endFromEnd'\^)?(?'end'\d+)?");

        if (!match.Success)
        {
            x.ErrorMessage = "invalid format, use format (\\^?)\\d+(..)?(\\^?)(\\d+)?";
            return new Range();
        }

        var start = int.Parse(match.Groups["start"].Value);
        var startIndex = match.Groups["startFromEnd"].Success ? new Index(start, true) : new Index(start);

        if (!match.Groups["isRange"].Success)
        {
            return new Range(startIndex, startIndex);
        }

        var endIndex = Index.FromEnd(0);
        if (match.Groups["end"].Success)
        {
            var end = int.Parse(match.Groups["end"].Value);
            endIndex = match.Groups["endFromEnd"].Success ? new Index(end, true) : new Index(end);
        }

        return new Range(startIndex, endIndex);
    });

    static AppOptions()
    {
        Range.SetDefaultValue(new Range(Index.FromStart(0), Index.FromEnd(0)));
        Range.Arity = ArgumentArity.ZeroOrOne;
        ProviderType.AddCompletions(Enum.GetNames<ProviderType>());
    }
}
