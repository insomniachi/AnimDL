using AnimDL.Api;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace AnimDL.Media;

public class MediaPlayer : IMediaPlayer
{
    public string Executable { get; }
    public bool IsAvailable { get; }

    public MediaPlayer(IConfiguration configuration)
    {
        Executable = configuration["MediaPlayer"];
        IsAvailable = File.Exists(Executable);
    }

    public Task Play(string streamUri, string title)
    {
        Process.Start(Executable, $"{streamUri} --meta-title {title}");

        return Task.CompletedTask;
    }
}
