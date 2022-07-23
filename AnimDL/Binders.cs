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

public class InstanceBinder<T> : BinderBase<T>
{
    private readonly T _instance;

    public InstanceBinder(T instance)
    {
        _instance = instance;
    }

    protected override T GetBoundValue(BindingContext bindingContext)
    {
        return _instance;
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

public class MediaPlayerBinder : BinderBase<IMediaPlayer>
{
    private readonly Option<MediaPlayerType> _mediaPlayerType;

    public MediaPlayerBinder(Option<MediaPlayerType> mediaPlayerType)
    {
        _mediaPlayerType = mediaPlayerType;
    }

    protected override IMediaPlayer GetBoundValue(BindingContext bindingContext)
    {
        return Program.Resolve<IMediaPlayerFactory>().GetMediaPlayer(bindingContext.ParseResult.GetValueForOption(_mediaPlayerType));
    }
}
