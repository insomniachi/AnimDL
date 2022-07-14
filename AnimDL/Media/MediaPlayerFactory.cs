using AnimDL.Api;

namespace AnimDL.Media
{
    public class MediaPlayerFactory : IMediaPlayerFactory
    {
        private readonly IEnumerable<IMediaPlayer> _mediaPlayers;

        public MediaPlayerFactory(IEnumerable<IMediaPlayer> mediaPlayers)
        {
            _mediaPlayers = mediaPlayers;
        }

        public IMediaPlayer GetMediaPlayer(MediaPlayerType type) => _mediaPlayers.First(x => x.Type == type);
    }
}
