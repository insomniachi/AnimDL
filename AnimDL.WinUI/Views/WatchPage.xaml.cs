using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AnimDL.Core.Models;
using AnimDL.WinUI.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Linq;
using AnimDL.WinUI.Helpers;
using System.Net.Http;
using System.IO;

namespace AnimDL.WinUI.Views;

public class WatchPageBase : ReactivePage<WatchViewModel> { }

public sealed partial class WatchPage : WatchPageBase
{
    private readonly HttpClient _client = new();

    public WatchPage()
    {
        ViewModel = App.GetService<WatchViewModel>();
        InitializeComponent();
        // ComboBox
        this.OneWayBind(ViewModel, vm => vm.Providers, view => view.Providers.ItemsSource);
        this.Bind(ViewModel, vm => vm.SelectedProviderType, view => view.Providers.SelectedItem);

        // AutoSuggesBox
        this.OneWayBind(ViewModel, vm => vm.SearchResult, view => view.SearchBox.ItemsSource);
        this.Bind(ViewModel, vm => vm.Query, view => view.SearchBox.Text);

        // Episode List
        this.OneWayBind(ViewModel, vm => vm.Episdoes, view => view.EpisodeList.ItemsSource);

        this.WhenActivated(d =>
        {
            // Update suggestions
            SearchBox.Events().TextChanged
                              .Select(x => x.sender.Text.Trim())
                              .Where(x => x.Length > 3)
                              .Throttle(TimeSpan.FromMilliseconds(800))
                              .DistinctUntilChanged()
                              .ObserveOn(RxApp.MainThreadScheduler)
                              .Subscribe(ViewModel.Search)
                              .DisposeWith(d);

            // Clear suggestions
            SearchBox.Events().TextChanged
                              .Select(x => x.sender.Text.Trim())
                              .Where(x => x.Length <= 3)
                              .SubscribeOn(RxApp.MainThreadScheduler)
                              .Subscribe(_ => ViewModel.SearchResult.Clear());

            // Suggestion Choosen
            SearchBox.Events().SuggestionChosen
                              .Select(x => x.args.SelectedItem as SearchResult)
                              .Subscribe(ViewModel.FetchEpisodes);

            // Episode Choosen
            EpisodeList.Events().SelectionChanged
                                .Where(x => x.AddedItems.Count == 1)
                                .Select(x => x.AddedItems[0] as VideoStreamsForEpisode)
                                .Subscribe(async x =>
                                {
                                    var url = x.Qualities.Values.ElementAt(0).Url;
                                    var html = VideoJsHelper.GetPlayerHtml(url);
                                    await WebView.EnsureCoreWebView2Async();
                                    WebView.NavigateToString(html);
                                });
        });
    }

    public void Execute(Action action)
    {
        DispatcherQueue.TryEnqueue(() => action());
    }
}
