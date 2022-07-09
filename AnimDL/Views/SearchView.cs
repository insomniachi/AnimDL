using AnimDL.Api;
using AnimDL.ViewModels;
using Terminal.Gui;
using ReactiveMarbles.ObservableEvents;
using ReactiveUI;
using System.Reactive.Linq;

namespace AnimDL.Views;

public class SearchDialog : BaseDialog<SearchViewModel>
{
    public SearchDialog(SearchViewModel vm) :base("Search", vm)
    {
		AddButtons();
    }

	public void AddButtons()
    {
		var searchButton = new Button("Search");
		var grabButton = new Button("Grab");
		var streamButton = new Button("Stream");
		searchButton.Events()
					.Clicked
					.InvokeCommand(ViewModel, x => x.SearchCommand);
		grabButton.Events()
				  .Clicked
				  .InvokeCommand(ViewModel, x => x.GrabCommand);
		streamButton.Events()
					.Clicked
					.InvokeCommand(ViewModel, x => x.StreamCommand);

		AddButton(searchButton);
		AddButton(grabButton);
		AddButton(streamButton);
    }

    public override void Setup()
    {
		Width = Dim.Sized(60);
		Height = Dim.Sized(10);

		var label = new Label("Provider")
		{
			X = Pos.Center() - Pos.Percent(45),
			Y = Pos.Center() - Pos.Percent(40)
		};

		var cbProviders = new ComboBox
		{
			X = label.X + 20,
			Y = label.Y,
			Width = Dim.Percent(50)
		};

		var tfQuery = new TextField
		{
			X = cbProviders.X,
			Y = cbProviders.Y + 3,
			Width = Dim.Percent(50)
		};

		var label2 = new Label("Query")
		{
			X = label.X,
			Y = tfQuery.Y
		};

		cbProviders.SetSource(ViewModel.Sources);
		cbProviders.Events()
				   .SelectedItemChanged
				   .Select(x => x.Item)
				   .DistinctUntilChanged()
				   .BindTo(ViewModel, x => x.SelectedProviderIndex);

		tfQuery.Events()
			   .TextChanged
			   .Select(x => tfQuery.Text)
			   .DistinctUntilChanged()
			   .BindTo(ViewModel, x => x.Query);

		Add(cbProviders, label, label2, tfQuery);
    }
}
