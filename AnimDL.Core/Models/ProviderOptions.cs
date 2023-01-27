using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace AnimDL.Core.Models;


public class ProviderOption : ReactiveObject
{
    private string _value = string.Empty;

    required public string Name { get; init; }
    required public string DisplayName { get; init; }
    required public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }
}

public class SelectableProviderOption : ProviderOption
{
    required public IEnumerable<string> AllowedValues { get; init; }
}

public class ProviderOptions : Collection<ProviderOption>
{
    public ProviderOptions AddOption(string name, string displayName, string value)
    {
        Add(new ProviderOption
        {
            Name = name,
            DisplayName = displayName,
            Value = value
        });

        return this;
    }

    public ProviderOptions AddSelectableOption(string name, string displayName, string value, IEnumerable<string> options)
    {
        Add(new SelectableProviderOption
        {
            Name = name,
            DisplayName = displayName,
            Value = value,
            AllowedValues = options
        });

        return this;
    }

    public bool TrySetValue(string name, string value)
    {
        if (this.FirstOrDefault(x => x.Name == name) is not { } option)
        {
            return false;
        }

        option.Value = value;
        return true;
    }

    public IObservable<ProviderOption> WhenChanged() => Observable.Merge(this.Select(x => x.WhenAnyPropertyChanged(nameof(ProviderOption.Value))))!;
    public string GetString(string name, string defaultValue) => GetValue(name, defaultValue, x => x);
    public int GetInt32(string name, int defaultValue) => GetValue(name, defaultValue, int.Parse);
    public double GetDouble(string name, double defaultValue) => GetValue(name, defaultValue, double.Parse);

    private T GetValue<T>(string name, T defaultValue, Func<string, T> parser)
    {
        if (this.FirstOrDefault(x => x.Name == name) is not { } option)
        {
            return defaultValue;
        }

        return parser(option.Value);
    }
}
