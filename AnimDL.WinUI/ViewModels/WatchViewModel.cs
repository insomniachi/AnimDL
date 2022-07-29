using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AnimDL.Api;
using AnimDL.Core.Models;
using AnimDL.WinUI.Contracts.ViewModels;
using AnimDL.WinUI.Views;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;

public class WatchViewModel : ReactiveObject, INavigationAware
{
    protected CompositeDisposable Garbage { get; } = new CompositeDisposable();

    private readonly SourceCache<SearchResult, string> _searchResultCache = new(x => x.Title);
    private readonly ReadOnlyObservableCollection<SearchResult> _searchResults;

    public WatchViewModel(IProviderFactory providerFactory)
    {

        SearchResultPicked = ReactiveCommand.CreateFromTask<SearchResult>(FetchEpisodes);

        this.WhenAnyValue(x => x.SelectedProviderType)
            .Subscribe(x => Provider = providerFactory.GetProvider(x));

        this.WhenAnyValue(x => x.Query)
            .Throttle(TimeSpan.FromMilliseconds(500), RxApp.TaskpoolScheduler)
            .SelectMany(async x => await Provider.Catalog.Search(x).ToListAsync())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => 
            {
                if(x.Count == 1)
                {
                    SearchResultPicked.Execute(x[0]);
                }
                else
                {
                    _searchResultCache.EditDiff(x, (first, second) => first.Title == second.Title);
                    IsSuggestionListOpen = true;
                }
            })
            .DisposeWith(Garbage);

        _searchResultCache
            .Connect()
            .RefCount()
            .Sort(SortExpressionComparer<SearchResult>.Ascending(x => x.Title))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _searchResults)
            .Subscribe(_ => { }, RxApp.DefaultExceptionHandler.OnNext)
            .DisposeWith(Garbage);
    }

    [Reactive] public string Query { get; set; }
    [Reactive] public ProviderType SelectedProviderType { get; set; } = ProviderType.AnimixPlay;
    [Reactive] public ObservableCollection<int> Episdoes { get; set; } = new();
    [Reactive] public string Url { get; set; }
    [Reactive] public bool IsSuggestionListOpen { get; set; }

    public ReadOnlyObservableCollection<SearchResult> SearchResult => _searchResults;
    public SearchResult SelectedResult { get; set; }
    public List<ProviderType> Providers { get; set; } = Enum.GetValues<ProviderType>().Cast<ProviderType>().ToList();
    public IProvider Provider { get; private set; }

    public ReactiveCommand<SearchResult, Unit> SearchResultPicked { get; }

    public void OnVideoPlayerMessageRecieved(VideoPlayerMessage message)
    {

    }

    public async Task FetchEpisodes(SearchResult result)
    {
        Episdoes.Clear();
        var count = await Provider.StreamProvider.GetNumberOfStreams(result.Url);
        Observable.Range(1, count)
                  .ObserveOn(RxApp.MainThreadScheduler)
                  .Subscribe(x => Episdoes.Add(x));
        SelectedResult = result;
    }

    public async Task FetchUrlForEp(int ep)
    {
        var epStream = await Provider.StreamProvider.GetStreams(SelectedResult.Url, ep..ep).ToListAsync();
        Url = epStream[0].Qualities.Values.ElementAt(0).Url;
    }

    public Task OnNavigatedTo(IReadOnlyDictionary<string, object> parameters)
    {
        IsSuggestionListOpen = false;
        if(parameters.ContainsKey("Title"))
        {
            Query = parameters["Title"] as string;
        }

        return Task.CompletedTask;
    }

    public Task OnNavigatedFrom()
    {
        Garbage.Dispose();
        return Task.CompletedTask;
    }
}
