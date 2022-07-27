using System.Collections.Generic;
using System.Threading.Tasks;
using AnimDL.WinUI.Contracts.ViewModels;
using ReactiveUI;

namespace AnimDL.WinUI.ViewModels;

public abstract class ViewModel : ReactiveObject, INavigationAware
{
    public virtual Task OnNavigatedFrom() => Task.CompletedTask;
    public virtual Task OnNavigatedTo(IReadOnlyDictionary<string, object> parameters) => Task.CompletedTask;
}
