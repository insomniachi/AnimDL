using AnimDL.ViewModels;
using Terminal.Gui;
using ReactiveMarbles.ObservableEvents;
using System.Reactive.Linq;
using ReactiveUI;
using AnimDL.Core.Models;

namespace AnimDL.Views
{
    public class StreamsDialog : BaseDialog<StreamsViewModel>
    {
        public StreamsDialog(StreamsViewModel vm): base("Episodes", vm) { }

        public override void Setup()
        {
            Width = Dim.Sized(100);
            Height = Dim.Sized(15);

            var listView = new ListView()
            {
                X = 1,
                Y = 2,
                Height = Dim.Fill(2),
                Width = Dim.Fill(2),
            };
            listView.SetSource(ViewModel.Episodes);
            Add(listView);
           
            var scrollBar = new ScrollBarView(listView, true);
            scrollBar.ChangedPosition += () => {
                listView.TopItem = scrollBar.Position;
                if (listView.TopItem != scrollBar.Position)
                {
                    scrollBar.Position = listView.TopItem;
                }
                listView.SetNeedsDisplay();
            };

            scrollBar.OtherScrollBarView.ChangedPosition += () => {
                listView.LeftItem = scrollBar.OtherScrollBarView.Position;
                if (listView.LeftItem != scrollBar.OtherScrollBarView.Position)
                {
                    scrollBar.OtherScrollBarView.Position = listView.LeftItem;
                }
                listView.SetNeedsDisplay();
            };


            listView.DrawContent += (e) => {
                scrollBar.Size = listView.Source.Count == 0 ? 0 : listView.Source.Count - 1;
                scrollBar.Position = listView.TopItem;
                scrollBar.OtherScrollBarView.Size = listView.Maxlength - 1;
                scrollBar.OtherScrollBarView.Position = listView.LeftItem;
                scrollBar.Refresh();
            };

            listView.Events()
                     .SelectedItemChanged
                     .Select(x => x.Item)
                     .DistinctUntilChanged()
                     .BindTo(ViewModel, x => x.SelectedIndex);
        }
    }
}
