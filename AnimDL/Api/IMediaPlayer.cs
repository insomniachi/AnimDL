namespace AnimDL.Api;

public interface IMediaPlayer
{
    void Play(string streamUri);
    bool IsAvailable { get; }
}
