using AnimDL.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;

namespace AnimDL.ViewModels;

public class StreamsViewModel : ReactiveObject
{
    [Reactive]
    public int SelectedIndex { get; set; }
    public List<string> Episodes { get; } = new();
}
