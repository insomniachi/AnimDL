using AnimDL.Api;
using AnimDL.Core;
using AnimDL.Core.Models;
using CliWrap;
using CliWrap.Buffered;
using Microsoft.Extensions.Configuration;

namespace AnimDL.Media;

public class VlcMediaPlayer : IMediaPlayer
{
    public string Executable { get; }
    public bool IsAvailable { get; }
    public MediaPlayerType Type => MediaPlayerType.Vlc;

    public VlcMediaPlayer(IConfiguration configuration)
    {
        Executable = Environment.ExpandEnvironmentVariables(configuration["MediaPlayers:VLC:Executable"]!);
        IsAvailable = File.Exists(Executable);
    }

    public async Task Play(VideoStream stream, string title)
    {
        var args = $"{stream.Url} --meta-title \"{title}\" --play-and-exit";

        if(stream.Headers.TryGetValue(Headers.Referer, out string? value))
        {
            args += $" --http-referrer={value}";
        }
        
        _ = await Cli.Wrap(Executable)
            .WithArguments(args)
            .ExecuteBufferedAsync();
    }
}
