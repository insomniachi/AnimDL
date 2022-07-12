using AnimDL.Api;
using System.CommandLine;
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

public class ProviderBinder : BinderBase<IProvider>
{
    private readonly Option<ProviderType> _providerType;

    public ProviderBinder(Option<ProviderType> providerType)
    {
        _providerType = providerType;
    }

    protected override IProvider GetBoundValue(BindingContext bindingContext)
    {
        return Program.Resolve<IProviderFactory>().GetProvider(bindingContext.ParseResult.GetValueForOption(_providerType));
    }
}
