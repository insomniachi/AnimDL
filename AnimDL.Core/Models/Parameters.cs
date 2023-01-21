using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace AnimDL.Core.Models;


public interface IParameters : IDictionary<string,object>
{
    T GetValue<T>(string key, T defaultValue);
    bool TryGetValue<T>(string key, out T? value);
}

public class Parameters : IParameters
{
    private readonly Dictionary<string, object> _parameters = new();

    public ICollection<string> Keys => _parameters.Keys;
    public ICollection<object> Values => _parameters.Values;
    public int Count => _parameters.Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<string, object>>)_parameters).IsReadOnly;
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _parameters.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public T GetValue<T>(string key, T defaultValue)
    {
        return _parameters.TryGetValue(key, out var value)
            ? (T)value
            : defaultValue;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        var result = _parameters.TryGetValue(key, out var obj);
        value = default;
        if(result && obj is not null)
        {
            value = (T)obj;
        }

        return result;
    }

    public object this[string key]
    {
        get => _parameters[key];
        set => _parameters[key] = value;
    }

    public bool ContainsKey(string key) => _parameters.ContainsKey(key);
    public void Add(KeyValuePair<string, object> item) => _parameters.Add(item.Key, item.Value);
    public void Add(string key, object value) => _parameters.Add(key, value);
    public bool Remove(string key) => _parameters.Remove(key);
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _parameters.TryGetValue(key, out value);
    public void Clear() => _parameters.Clear();
    public bool Contains(KeyValuePair<string, object> item) => _parameters.Contains(item);
    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string,object>>)_parameters).CopyTo(array, arrayIndex);
    public bool Remove(KeyValuePair<string, object> item) => ((ICollection<KeyValuePair<string, object>>)_parameters).Remove(item);
}
