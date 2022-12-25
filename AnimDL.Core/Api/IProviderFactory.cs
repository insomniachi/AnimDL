namespace AnimDL.Core.Api;

public interface IProviderFactory
{
    IProvider GetProvider(ProviderType type);
}
