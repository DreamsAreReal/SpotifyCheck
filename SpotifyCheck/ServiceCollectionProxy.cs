namespace SpotifyCheck;

public static class ServiceCollectionProxy
{
    public static IServiceProvider Provider { get; private set; }

    public static void SetServiceProvider(IServiceProvider provider)
    {
        if (Provider != null) throw new Exception("Provider set");
        Provider = provider;
    }
}