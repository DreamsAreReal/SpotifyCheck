using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyCheck.RuCaptcha.Configuration;
using SpotifyCheck.RuCaptcha.Exceptions;

namespace SpotifyCheck.RuCaptcha;

public static class Startup
{
    public static IServiceCollection AddRuCaptchaStartup(this IServiceCollection collection, IConfiguration configuration)
    {
        collection.Configure<RuCaptchaOptions>(configuration);
        collection.AddSingleton<RuCaptchaErrorCodes>();
        collection.AddSingleton<CaptchaMessageHandler>();
        collection.AddSingleton<CaptchaResultMessageHandler>();
        collection.AddTransient<RuCaptchaWrapper>();
        return collection;
    }

    public static IConfigurationBuilder AddRuCaptchaConfigs(this IConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddJsonFile("RuCaptchaOptions.json");
        return configurationBuilder;
    }
}