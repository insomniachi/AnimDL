using AnimDL.WinUI.ViewModels;
using Microsoft.UI.Xaml.Controls;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System;
using System.Reactive.Disposables;

namespace AnimDL.WinUI.Views;

public class MainPageBase : ReactivePage<MainViewModel> { }
public sealed partial class MainPage
{
    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();

        this.OneWayBind(ViewModel, vm => vm.SeasonalAnime, view => view.GridView.ItemsSource);

        this.WhenActivated(d =>
        {
            GridView.Events().ItemClick.Subscribe(x => ViewModel.ItemClicked.Execute(x.ClickedItem)).DisposeWith(d);
        });
    }
}
