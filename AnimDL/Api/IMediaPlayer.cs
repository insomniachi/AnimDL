using AnimDL.Core.Models;

namespace AnimDL.Api;

public interface IMediaPlayer
{
    Task Play(VideoStream stream, string title);
    bool IsAvailable { get; }
    MediaPlayerType Type { get; }
}

public interface IMediaPlayerFactory
{
    IMediaPlayer GetMediaPlayer(MediaPlayerType type);
}

public enum MediaPlayerType
{
    Vlc
}