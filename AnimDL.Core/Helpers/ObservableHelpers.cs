using System.Reactive.Linq;

namespace AnimDL.Core.Helpers;

public static class ObservableHelpers
{
    public static void TryWait<T>(this IObservable<T> observable)
    {
        try
        {
            observable.Wait();
        }
        catch { }
    }
}
