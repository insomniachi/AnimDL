namespace AnimDL.Api;

public interface IMediaPlayer
{
    Task Play(string streamUri, string title);
    bool IsAvailable { get; }
}
