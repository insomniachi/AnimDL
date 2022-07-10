using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AnimDL.ViewModels;

public class SearchResultViewModel : ReactiveObject
{
    public List<SearchResult> ActualResults { get; set; } = new();

    [Reactive]
    public int SelectedIndex { get; set; }
    public ICommand StreamAnimeCommand { get; set; }
    public IProvider? Provider { get; set; }

    public SearchResultViewModel()
    {
        StreamAnimeCommand = ReactiveCommand.Create(async () => await ExecuteStream());
    }

    private async Task ExecuteStream()
    {
        var item = ActualResults.ElementAt(SelectedIndex);

        if (Provider is null) return;

        await Program.ShowDialogAsync<StreamsDialog, StreamsViewModel>(async x =>
        {
            var count = await Provider.StreamProvider.GetNumberOfStreams(item.Url);
            x.Episodes.AddRange(Enumerable.Range(1, count).Select(x => "Episode " + x));
        });
    }
}
