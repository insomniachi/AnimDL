using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnimDL.WinUI.Core.Contracts;
using AnimDL.WinUI.Helpers;
using DynamicData;
using MalApi;
using MalApi.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;

public class SeasonalViewModel : ViewModel, IHaveState
{
    private readonly IMalClient _client;
    private readonly SourceCache<Anime, long> _animeCache = new(x => x.ID);
    private readonly ReadOnlyObservableCollection<Anime> _anime;

    public SeasonalViewModel(IMalClient client)
    {
        _client = client;

        SetSeasonCommand = ReactiveCommand.Create<string>(SwitchSeasonFilter);
        AddToListCommand = ReactiveCommand.Create<Anime>(AddToList);

        var filter = this.WhenAnyValue(x => x.Season).Select(FilterBySeason);

        _animeCache
            .Connect()
            .RefCount()
            .Filter(filter)
            .Bind(out _anime)
            .DisposeMany()
            .Subscribe(_ => { }, RxApp.DefaultExceptionHandler.OnNext)
            .DisposeWith(Garbage);

        this.WhenAnyValue(x => x.SeasonFilter).Subscribe(SwitchSeasonFilter);
    }

    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public Season Season { get; set; }
    [Reactive] public string SeasonFilter { get; set; } = "Current";

    public Season Current { get; private set; }
    public Season Next { get; private set; }
    public Season Prev { get; private set; }
    public ReadOnlyObservableCollection<Anime> Anime => _anime;
    public ICommand SetSeasonCommand { get; }
    public ICommand AddToListCommand { get; }

    public async Task SetInitialState()
    {
        IsLoading = true;

        var (prevSeason, prevYear) = AnimeHelpers.PrevSeason();
        var (currSeason, currYear) = AnimeHelpers.CurrentSeason();
        var (nextSeason, nextYear) = AnimeHelpers.NextSeason();

        Prev = Season.Get(prevSeason, prevYear);
        Current = Season.Get(currSeason, currYear);
        Next = Season.Get(nextSeason, nextYear);
        Season = Current;

        var currentAnime = await _client.Anime().OfSeason(currSeason, currYear)
                                        .WithFields("start_season", FieldName.UserStatus).Find();
        
        _animeCache.Edit(x =>
        {
            x.AddOrUpdate(currentAnime.Data);
        });
        
        var prevAnime = await _client.Anime().OfSeason(prevSeason, currYear)
                                     .WithFields("start_season", FieldName.UserStatus).Find();
        var nextAnime = await _client.Anime().OfSeason(nextSeason, nextYear)
                                     .WithFields("start_season", FieldName.UserStatus).Find();

        _animeCache.Edit(x =>
        {
            x.AddOrUpdate(prevAnime.Data);
            x.AddOrUpdate(nextAnime.Data);
        });

        IsLoading = false;
    }

    public void StoreState(IState state)
    {
        state.AddOrUpdate(Anime);
        state.AddOrUpdate(Season);
    }

    public void RestoreState(IState state)
    {
        var anime = state.GetValue<ReadOnlyObservableCollection<Anime>>(nameof(Anime));
        _animeCache.Edit(x => x.AddOrUpdate(anime));

        Season = state.GetValue<Season>(nameof(Season));
    }

    private void SwitchSeasonFilter(string filter)
    {
        Season = filter switch
        {
            "Current" => Current,
            "Previous" => Prev,
            "Next" => Next,
            _ => throw new InvalidOperationException()
        };
    }

    private void AddToList(Anime a) { }

    private Func<Anime, bool> FilterBySeason(Season s) => x => x.StartSeason.SeasonName == s.SeasonName && x.StartSeason.Year == s.Year;

}
