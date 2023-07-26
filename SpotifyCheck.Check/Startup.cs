using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SpotifyCheck.Check;

public static class Startup
{
    public static IServiceCollection AddSpotifyStartup(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.AddTransient<IBrowserAuthorizationWrapper, FirefoxAuthorizationWrapper>();
        collection.AddTransient<AbstractSpotifyWrapper, BrowserSpotifyWrapper>();
        return collection;
    }

    public static IConfigurationBuilder AddSpotifyConfigs(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("BrowserOptions.json");
        configurationBuilder.AddJsonFile("SpotifyOptions.json");
        return configurationBuilder;
    }
}