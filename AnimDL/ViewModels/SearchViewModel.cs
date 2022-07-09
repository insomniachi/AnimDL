using AnimDL.Api;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Windows.Input;

namespace AnimDL.ViewModels
{
    public class SearchViewModel : ReactiveObject
    {
        public List<string> Sources { get; } = Enum.GetNames<ProviderType>().ToList();
       
        [Reactive]
        public string Query { get; set; } = string.Empty;

        [Reactive]
        public int SelectedProviderIndex { get; set; }

        public ICommand SearchCommand { get; }
        public ICommand GrabCommand { get; }
        public ICommand StreamCommand { get; }

        public SearchViewModel()
        {
            SearchCommand = ReactiveCommand.Create(() => Search());
            GrabCommand = ReactiveCommand.Create(() => Grab());
            StreamCommand = ReactiveCommand.Create(() => Stream());
        }

        private void Search()
        {

        }

        private void Grab()
        {

        }

        private void Stream()
        {

        }

    }
}
