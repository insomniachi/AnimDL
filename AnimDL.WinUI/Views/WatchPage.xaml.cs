using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnimDL.Core.Models;
using AnimDL.WinUI.Helpers;
using AnimDL.WinUI.ViewModels;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace AnimDL.WinUI.Views;

public class WatchPageBase : ReactivePage<WatchViewModel> { }

public sealed partial class WatchPage : WatchPageBase
{
    public WatchPage()
    {
        ViewModel = App.GetService<WatchViewModel>();
        InitializeComponent();

        var resut = JsonSerializer.Serialize(new VideoPlayerMessage { MessageType = VideoPlayerMessageType.DurationUpdate, Content = "12.402"});
        // ComboBox
        this.OneWayBind(ViewModel, vm => vm.Providers, view => view.Providers.ItemsSource);
        this.Bind(ViewModel, vm => vm.SelectedProviderType, view => view.Providers.SelectedItem);

        // AutoSuggesBox
        this.OneWayBind(ViewModel, vm => vm.SearchResult, view => view.SearchBox.ItemsSource);
        this.Bind(ViewModel, vm => vm.Query, view => view.SearchBox.Text);
        this.Bind(ViewModel, vm => vm.IsSuggestionListOpen, view => view.SearchBox.IsSuggestionListOpen);

        // Episode List
        this.OneWayBind(ViewModel, vm => vm.Episdoes, view => view.EpisodeList.ItemsSource);

        // Load video html
        this.ObservableForProperty(x => x.ViewModel.Url, x => x)
            .Subscribe(async x =>
            {
                var html = VideoJsHelper.GetPlayerHtml(x);
                await WebView.EnsureCoreWebView2Async();
                WebView.NavigateToString(html);
            });

        this.WhenActivated(d =>
        {
            // Suggestion Choosen
            SearchBox.Events().SuggestionChosen
                              .Select(x => x.args.SelectedItem as SearchResult)
                              .InvokeCommand(ViewModel.SearchResultPicked)
                              .DisposeWith(d);

            // Episode Choosen
            EpisodeList.Events().SelectionChanged
                                .Where(x => x.AddedItems.Count == 1)
                                .Select(x => (int)x.AddedItems[0])
                                .Subscribe(async x => await ViewModel.FetchUrlForEp(x))
                                .DisposeWith(d);

            WebView.Events()
                   .WebMessageReceived
                   .Select(x => JsonSerializer.Deserialize<VideoPlayerMessage>(x.args.WebMessageAsJson))
                   .Subscribe(ViewModel.OnVideoPlayerMessageRecieved)
                   .DisposeWith(d);
        });
    }
}

public enum VideoPlayerMessageType
{
    Ready,
    TimeUpdate,
    DurationUpdate
}

public class VideoPlayerMessage
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public VideoPlayerMessageType MessageType { get; set; }
    public string Content { get; set; }
}