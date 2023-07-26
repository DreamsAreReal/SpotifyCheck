// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpotifyCheck;
using SpotifyCheck.Check;

var configuration = new ConfigurationBuilder().AddConfigs().AddSpotifyConfigs().Build();
var services = new ServiceCollection().AddStartup(configuration).AddSpotifyStartup(configuration).BuildServiceProvider();
ServiceCollectionProxy.SetServiceProvider(services);
var workerMessageHandler = services.GetService<WorkerMessageHandler>();