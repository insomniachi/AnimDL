using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.Services;
using AnimDL.WinUI.Core.Contracts;
using DynamicData;
using MalApi;
using MalApi.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;


public class UserListViewModel : ViewModel, IHaveState
{
    private readonly IMalClient _malClient;
    private readonly INavigationService _navigationService;
    private readonly IViewService _viewService;
    private readonly SourceCache<Anime, long> _animeCache = new(x => x.Id);
    private readonly ReadOnlyObservableCollection<Anime> _anime;

    public UserListViewModel(IMalClient malClient,
                             INavigationService navigationService,
                             IViewService viewService)
    {
        _malClient = malClient;
        _navigationService = navigationService;
        _viewService = viewService;

        ItemClickedCommand = ReactiveCommand.Create<Anime>(OnItemClicked);
        ChangeCurrentViewCommand = ReactiveCommand.Create<AnimeStatus>(x => CurrentView = x);
        RefreshCommand = ReactiveCommand.CreateFromTask(SetInitialState);
        UpdateStatusCommand = ReactiveCommand.Create<Anime>(async x => await _viewService.UpdateAnimeStatus(x));

        var filter = this.WhenAnyValue(x => x.CurrentView)
                         .Select(FilterByStatusPredicate);

        _animeCache
            .Connect()
            .RefCount()
            .Filter(filter)
            .Bind(out _anime)
            .DisposeMany()
            .Subscribe(_ => { }, RxApp.DefaultExceptionHandler.OnNext)
            .DisposeWith(Garbage);
    }

    [Reactive] public AnimeStatus CurrentView { get; set; } = AnimeStatus.Watching;
    [Reactive] public bool IsLoading { get; set; }

    public ReadOnlyObservableCollection<Anime> UserAnime => _anime;
    public ICommand ItemClickedCommand { get; }
    public ICommand ChangeCurrentViewCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand UpdateStatusCommand { get; }

    private void OnItemClicked(Anime anime)
    {
        _navigationService.NavigateTo(typeof(WatchViewModel).FullName, new Dictionary<string, object> { ["Anime"] = anime });
    }

    private Func<Anime, bool> FilterByStatusPredicate(AnimeStatus status) => x => x.UserStatus.Status == status;

    public async Task SetInitialState()
    {
        IsLoading = true;
        _animeCache.Clear();
        var userAnime = await _malClient.Anime().OfUser().WithField(x => x.UserStatus).Find();
        _animeCache.AddOrUpdate(userAnime.Data);
        IsLoading = false;
    }

    public void StoreState(IState state)
    {
        state.AddOrUpdate(UserAnime);
        state.AddOrUpdate(CurrentView);
    }

    public void RestoreState(IState state)
    {
        var anime = state.GetValue<ReadOnlyObservableCollection<Anime>>(nameof(UserAnime));
        _animeCache.Edit(x => x.AddOrUpdate(anime));
        CurrentView = state.GetValue<AnimeStatus>(nameof(CurrentView));
    }
}
