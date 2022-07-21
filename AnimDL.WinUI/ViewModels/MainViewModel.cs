using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AnimDL.WinUI.Contracts.ViewModels;
using AnimDL.WinUI.Core.Contracts.Services;
using CommunityToolkit.WinUI.UI;
using JikanDotNet;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.ViewModels;

public class MainViewModel : ReactiveObject, INavigationAware
{
    private readonly IAnimeListService _animeListService;

    public MainViewModel(IAnimeListService animeListService)
    {
        _animeListService = animeListService;
        ItemClicked = ReactiveCommand.CreateFromTask<Anime>(OnItemClicked);
    }

    [Reactive]
    public AdvancedCollectionView SeasonalAnime { get; set; }
    
    public ICommand ItemClicked { get; }

    public void OnNavigatedFrom()
    {
    }

    public async void OnNavigatedTo(object parameter)
    {
        //SeasonalAnime = new(await _animeListService.GetAnimeSeason(2022, MalApi.AnimeSeason.Summer));
    }

    private Task OnItemClicked(Anime anime)
    {
        return Task.CompletedTask;
    }
}
