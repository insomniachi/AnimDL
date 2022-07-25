using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.Services;
using AnimDL.WinUI.Contracts.ViewModels;
using DynamicData;
using MalApi;
using MalApi.Interfaces;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;


public class UserListViewModel : ReactiveObject, INavigationAware
{
    protected CompositeDisposable Garbage { get; } = new CompositeDisposable();
    private readonly IMalClient _malClient;
    private readonly INavigationService _navigationService;
    private readonly SourceCache<Anime, long> _animeCache = new(x => x.ID);
    private readonly ReadOnlyObservableCollection<Anime> _anime;

    public UserListViewModel(IMalClient malClient,
                             INavigationService navigationService)
    {
        _malClient = malClient;
        _navigationService = navigationService;
        
        ItemClicked = ReactiveCommand.Create<Anime>(OnItemClicked);

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
    public ICommand ItemClicked { get; }


    public Task OnNavigatedFrom() => Task.CompletedTask;
    
    public async Task OnNavigatedTo(IReadOnlyDictionary<string, object> parameters)
    {
        IsLoading = true;
        var userAnime = await _malClient.Anime().OfUser().WithFields(FieldName.UserStatus).Find();
        _animeCache.AddOrUpdate(userAnime.Data);        
        IsLoading = false;
    }

    private void OnItemClicked(Anime anime)
    {
        _navigationService.NavigateTo(typeof(WatchViewModel).FullName, new Dictionary<string, object> { ["Title"] = anime.Title });
    }

    private Func<Anime, bool> FilterByStatusPredicate(AnimeStatus status) => x => x.UserStatus.Status == status;
}
