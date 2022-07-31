using System.Text.Json;
using AnimDL.WinUI.Core.Contracts;

namespace AnimDL.WinUI.Core.Services;

public class PlaybackStateStorage : IPlaybackStateStorage
{
    private readonly string _file = "recents.json";

    private readonly Dictionary<long, Dictionary<int, double>> _recents;

    public PlaybackStateStorage()
    {
        _recents = File.Exists(_file)
            ? JsonSerializer.Deserialize<Dictionary<long, Dictionary<int, double>>>(File.ReadAllText(_file)) ?? new()
            : new();
    }

    public double GetTime(long id, int episode)
    {
        if(!_recents.ContainsKey(id))
        {
            return 0;
        }

        if (!_recents[id].ContainsKey(episode))
        {
            return 0;
        }

        return _recents[id][episode];
    }

    public void Reset(long id, int episode)
    {
        if (!_recents.ContainsKey(id))
        {
            return;
        }

        if (!_recents[id].ContainsKey(episode))
        {
            return;
        }

        _recents[id].Remove(episode);

        if (_recents[id].Count == 0)
        {
            _recents.Remove(id);
        }
    }

    public void StoreState()
    {
        File.WriteAllText(_file, JsonSerializer.Serialize(_recents));
    }

    public void Update(long id, int episode, double time)
    {
        if (!_recents.ContainsKey(id))
        {
            _recents.Add(id, new Dictionary<int, double>() { [episode] = time });
            return;
        }

        if (!_recents[id].ContainsKey(episode))
        {
            _recents[id].Add(episode, time);
            return;
        }

        _recents[id][episode] = time;
    }
}
