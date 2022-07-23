using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
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
    public ProviderType SelectedProviderType { get; set; } = ProviderType.AnimixPlay;

    public ObservableCollection<SearchResult> SearchResult { get; set; } = new();
    
    public ObservableCollection<VideoStreamsForEpisode> Episdoes { get; set; } = new();

    public List<ProviderType> Providers { get; set; } = Enum.GetValues<ProviderType>().Cast<ProviderType>().ToList();

    public IProvider Provider { get; set; }

    public void Search(string query)
    {
        SearchResult.Clear();
        Provider.Catalog
                .Search(query)
                .ToObservable()
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SearchResult.Add(x), OnException);
    }

    public void FetchEpisodes(SearchResult result)
    {
        Episdoes.Clear();
        Provider.StreamProvider
                .GetStreams(result.Url, Range.All)
                .ToObservable()
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => Episdoes.Add(x), OnException);
    }

    private void OnException(Exception ex)
    {
        Console.WriteLine(ex);
    }

}
