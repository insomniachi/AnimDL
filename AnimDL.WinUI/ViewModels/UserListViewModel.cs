using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.Services;
using AnimDL.WinUI.Contracts.ViewModels;
using AnimDL.WinUI.Core.Contracts.Services;
using CommunityToolkit.WinUI.UI;
using MalApi;
using MalApi.Requests;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;


public class UserListViewModel : ReactiveObject, INavigationAware
{
    private readonly IAnimeListService _animeListService;
    private readonly ILocalSettingsService _localSettingsService;

    public UserListViewModel(IAnimeListService animeListService, ILocalSettingsService localSettingsService)
    {
        _animeListService = animeListService;
        _localSettingsService = localSettingsService;
        ItemClicked = ReactiveCommand.CreateFromTask<Anime>(OnItemClicked);

        this.WhenAnyValue(x => x.CurrentView)
            .Subscribe(x =>
            {
                UserAnime.Filter = y =>
                {
                    var anime = y as Anime;
                    return anime.UserStatus.Status == x;
                };
            });
    }

    [Reactive]
    public AdvancedCollectionView UserAnime { get; set; } = new();

    [Reactive]
    public AnimeStatus CurrentView { get; set; } = AnimeStatus.Watching;

    [Reactive]
    public bool IsLoading { get; set; }

    public ICommand ItemClicked { get; }

    public void OnNavigatedFrom()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
        IsLoading = true;

        var token = _localSettingsService.ReadSetting<OAuthToken>("MalToken");
        var client = new MalClient(token.AccessToken);

        var userAnime = await client.GetAnime()
                                    .OfUser()
                                    .WithFields(FieldName.UserStatus)
                                    .SortBy(Sort.Title)
                                    .Find();

        var update = await client.GetAnime()
                                 .WithId(42963)
                                 .UpdateStatus()
                                 .WithEpisodesWatched(4)
                                 .Publish();

        UserAnime = new(userAnime.Data)
        {
            Filter = y =>
            {
                var anime = y as Anime;
                return anime.UserStatus.Status == CurrentView;
            }
        };
        UserAnime.SortDescriptions.Add(new SortDescription("MeanScore", SortDirection.Descending));
        UserAnime.SortDescriptions.Add(new SortDescription("Title", SortDirection.Ascending));

        IsLoading = false;
    }

    private Task OnItemClicked(Anime anime)
    {
        return Task.CompletedTask;
    }
}
