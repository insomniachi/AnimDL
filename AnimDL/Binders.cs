using AnimDL.Api;
using AnimDL.Core;
using AnimDL.Core.Api;
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
    private readonly Option<string> _providerType;

    public ProviderBinder(Option<string> providerType)
    {
        _providerType = providerType;
    }

    protected override IProvider GetBoundValue(BindingContext bindingContext)
    {
        return ProviderFactory.Instance.GetProvider(bindingContext.ParseResult.GetValueForOption(_providerType)!) ?? throw new Exception();
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
