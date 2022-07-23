using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AnimDL.Api;
using AnimDL.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;

public class WatchViewModel : ReactiveObject
{

    public WatchViewModel(IProviderFactory providerFactory)
    {
        this.WhenAnyValue(x => x.SelectedProviderType)
            .Subscribe(x => Provider = providerFactory.GetProvider(x));
    }

    [Reactive]
    public string Query { get; set; }

    [Reactive]
    public List<SearchResult> SearchResult { get; set; }

    [Reactive]
    public ProviderType SelectedProviderType { get; set; } = ProviderType.AnimixPlay;

    public ObservableCollection<VideoStreamsForEpisode> Episdoes { get; set; } = new();

    public List<ProviderType> Providers { get; set; } = Enum.GetValues<ProviderType>().Cast<ProviderType>().ToList();

    public IProvider Provider { get; set; }

    public Action<Action> UIExecute { get; set; }

    public async Task Search(string query)
    {
        SearchResult = await Provider.Catalog.Search(query).ToListAsync();
    }

    public void FetchEpisodes(SearchResult result)
    {
        Task.Run(async () =>
        {
            await foreach (var epStream in Provider.StreamProvider.GetStreams(result.Url, Range.All))
            {
                UIExecute?.Invoke(() => Episdoes.Add(epStream));
            }
        });
    }

}
