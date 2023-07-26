using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyCheck.Configurations;

namespace SpotifyCheck;

public static class Startup
{
    public static IServiceCollection AddStartup(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.Configure<AppOptions>(configuration.GetSection("AppOptions"));
        collection.AddSingleton<WorkerMessageHandler>();
        return collection;
    }

    public static IConfigurationBuilder AddConfigs(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("AppOptions.json");
        return configurationBuilder;
    }
}