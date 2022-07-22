using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.ViewModels;
using AnimDL.WinUI.Core.Contracts.Services;
using CommunityToolkit.WinUI.UI;
using MalApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;


public class UserListViewModel : ReactiveObject, INavigationAware
{
    private readonly IAnimeListService _animeListService;
    public UserListViewModel(IAnimeListService animeListService)
    {
        _animeListService = animeListService;
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

        var userAnime = await _animeListService.GetUserAnime();
        UserAnime = new(userAnime.OrderBy(x => x.MeanScore).ToList())
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
