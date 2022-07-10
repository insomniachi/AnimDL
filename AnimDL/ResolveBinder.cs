using Microsoft.Extensions.DependencyInjection;
using System.CommandLine.Binding;

namespace AnimDL;

public class ResolveBinder<T> : BinderBase<T>
    where T : notnull
{


    protected override T GetBoundValue(BindingContext bindingContext)
    {
        return Program.Resolve<T>();
    }
}
