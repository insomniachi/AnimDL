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

        ViewModel.UIExecute += Execute;

        // ComboBox
        this.OneWayBind(ViewModel, vm => vm.Providers, view => view.Providers.ItemsSource);
        this.Bind(ViewModel, vm => vm.SelectedProviderType, view => view.Providers.SelectedItem);

        // AutoSuggesBox
        this.OneWayBind(ViewModel, vm => vm.SearchResult, view => view.SearchBox.ItemsSource);
        this.Bind(ViewModel, vm => vm.Query, view => view.SearchBox.Text);

        this.OneWayBind(ViewModel, vm => vm.Episdoes, view => view.EpisodeList.ItemsSource);

        this.WhenActivated(d =>
        {
            SearchBox.Events().TextChanged
                              .Where(x => x.sender.Text.Length > 3)
                              .Select(x => x.sender.Text)
                              .Throttle(TimeSpan.FromMilliseconds(800))
                              .ObserveOn(RxApp.MainThreadScheduler)
                              .Subscribe(async x => await ViewModel.Search(x))
                              .DisposeWith(d);

            SearchBox.Events().SuggestionChosen
                              .Select(x => x.args.SelectedItem as SearchResult)
                              .Subscribe(ViewModel.FetchEpisodes);

            EpisodeList.Events().SelectionChanged
                                .Where(x => x.AddedItems.Count == 1)
                                .Select(x => x.AddedItems[0] as VideoStreamsForEpisode)
                                .Subscribe(async x =>
                                {
                                    var url = x.Qualities.Values.ElementAt(0).Url;
                                    //File.WriteAllText("hls.m3u8", m3u8);
                                    var html = VideoJsHelper.GetPlayerHtml(url);
                                    //File.WriteAllText("player.html", html);
                                    await WebView.EnsureCoreWebView2Async();
                                    WebView.NavigateToString(html);
                                    //var playrePath = $"file://{Path.GetFullPath("player.html")}";
                                    //WebView.CoreWebView2.Navigate($"file://{Path.GetFullPath("player.html")}");
                                });
        });
    }

    public void Execute(Action action)
    {
        DispatcherQueue.TryEnqueue(() => action());
    }
}
