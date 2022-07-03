namespace AnimDL.Api;

public interface IProviderFactory
{
    IProvider GetProvider(ProviderType type);
}
