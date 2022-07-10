using AnimDL.Api;
using AnimDL.ViewModels;
using AnimDL.Views;
using NStack;
using ReactiveUI;
using Terminal.Gui;

namespace AnimDL;

public static class GuiRunner
{
    public static int Run()
    {
        Application.Init();
        var top = Application.Top;

        // Creates the top-level window to show
        var win = new Window("AnimDL")
        {
            X = 0,
            Y = 1, // Leave one row for the toplevel menu

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = Colors.TopLevel
        };

        top.Add(win);

        // Creates a menubar, the item "New" has a help menu.
        var menu = new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem ("_File", new MenuItem [] 
            {
                new MenuItem ("_Find", "Search for anime", async () => await Program.ShowDialogAsync<SearchDialog, SearchViewModel>(), null, null, Key.F | Key.CtrlMask),
                new MenuItem ("_Close", "Quit Applicaiton", () => Application.RequestStop(), null, null, Key.Q | Key.CtrlMask),
            }),
        });

        top.Add(menu);

        Application.Run();
        Application.Shutdown();

        return 0;
    }
}
