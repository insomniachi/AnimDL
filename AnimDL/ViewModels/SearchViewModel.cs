using AnimDL.Api;
using AnimDL.Commands;
using AnimDL.Views;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows.Input;

namespace AnimDL.ViewModels
{
    public class SearchViewModel : ReactiveObject
    {
        private readonly IProviderFactory _providerFactory;
        private readonly ILoggerFactory _loggerFactory;

        public List<string> Sources { get; } = Enum.GetNames<ProviderType>().ToList();
       
        [Reactive]
        public string Query { get; set; } = string.Empty;

        [Reactive]
        public int SelectedProviderIndex { get; set; }

        public ICommand SearchCommand { get; }

        public SearchViewModel(IProviderFactory providerFactory, ILoggerFactory loggerFactory)
        {
            _providerFactory = providerFactory;
            _loggerFactory = loggerFactory;

            SearchCommand = ReactiveCommand.Create(() => Search());
        }

        private async Task Search()
        {
            var providerType = Enum.GetValues<ProviderType>().Cast<ProviderType>().ElementAt(SelectedProviderIndex);
            var logger = _loggerFactory.CreateLogger<SearchCommand>();
            var results = Commands.SearchCommand.UiExecute(Query, _providerFactory.GetProvider(providerType));
            await Program.ShowDialogAsync<SearchResultDialog, SearchResultViewModel>(async x => 
            {
                x.Provider = _providerFactory.GetProvider(providerType);
                await foreach (var item in results)
                {
                    x.ActualResults.Add(item);
                }
            });
        }
    }
}
