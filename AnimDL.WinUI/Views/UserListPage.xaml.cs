using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AnimDL.WinUI.ViewModels;
using MalApi;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;

namespace AnimDL.WinUI.Views;

public class UserListPageBase : ReactivePage<UserListViewModel> { }
public sealed partial class UserListPage : UserListPageBase
{
    public UserListPage()
    {
        ViewModel = App.GetService<UserListViewModel>();
        InitializeComponent();

        this.OneWayBind(ViewModel, vm => vm.UserAnime, view => view.AnimeListView.ItemsSource);
        this.OneWayBind(ViewModel, vm => vm.IsLoading, view => view.LoadingControl.IsLoading);

        this.WhenActivated(d =>
        {
            AnimeListView.Events().ItemClick.Select(x => x.ClickedItem as Anime).InvokeCommand(ViewModel.ItemClicked);
            
            WatchingToggleBtn.Events().Checked.Subscribe(_ => ViewModel.CurrentView = AnimeStatus.Watching).DisposeWith(d);
            PtwToggleBtn.Events().Checked.Subscribe(_ => ViewModel.CurrentView = AnimeStatus.PlanToWatch).DisposeWith(d);
            CompletedToggleBtn.Events().Checked.Subscribe(_ => ViewModel.CurrentView = AnimeStatus.Completed).DisposeWith(d);
            OnHoldToggleBtn.Events().Checked.Subscribe(_ => ViewModel.CurrentView = AnimeStatus.OnHold).DisposeWith(d);
            DroppedToggleBtn.Events().Checked.Subscribe(_ => ViewModel.CurrentView = AnimeStatus.Dropped).DisposeWith(d);
        });
    }
}
