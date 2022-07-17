using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System;

namespace AnimDL.Gui.Services;

public class NavigationService
{
    public static NavigationService Instance { get; } = new NavigationService();

    public void SetFrame(Frame f)
    {
        _frame = f;
    }

    public void SetOverlayHost(Panel p)
    {
        _overlayHost = p;
    }

    public void Navigate(Type t)
    {
        _frame.Navigate(t);
    }

    public void ClearOverlay()
    {
        _overlayHost?.Children.Clear();

    }

    private Frame _frame;
    private Panel _overlayHost;
}