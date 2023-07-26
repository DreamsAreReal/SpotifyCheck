using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyCheck.Check;
using SpotifyCheck.Check.Exceptions;
using SpotifyCheck.Configurations;
using SpotifyCheck.Core;
using SpotifyCheck.Core.Models;
using SpotifyCheck.Models;
using InvalidDataException = SpotifyCheck.Check.Exceptions.InvalidDataException;

namespace SpotifyCheck;

public class WorkerMessageHandler : AbstractMessageHandler<WorkerContext, AccountSuccess, Account>
{
    private readonly AppOptions _appOptions;
    private readonly ILogger<WorkerMessageHandler> _logger;
    private readonly ConcurrentQueue<Proxy> _proxies;
    private readonly SemaphoreSlim _semaphoreSlim;

    public WorkerMessageHandler(ILogger<WorkerMessageHandler> logger, IOptions<AppOptions> appOptions) : base(logger)
    {
        _logger = logger;
        _appOptions = appOptions.Value;
        _semaphoreSlim = new SemaphoreSlim(_appOptions.MaxThreads, _appOptions.MaxThreads);
        _proxies = new ConcurrentQueue<Proxy>();
    }

    public void AddProxy(Proxy proxy)
    {
        _proxies.Enqueue(proxy);
    }

    protected override async Task Handle(WorkerContext message, CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        var spotifyWrapper = ServiceCollectionProxy.Provider.GetService<AbstractSpotifyWrapper>();
        new Thread(async () => { await HandleMessage(spotifyWrapper!, message, _semaphoreSlim, cancellationToken); }).Start();
    }

    private async Task HandleMessage(
        AbstractSpotifyWrapper wrapper,
        WorkerContext workerContext,
        SemaphoreSlim semaphoreSlim,
        CancellationToken ctx
    )
    {
        if (ctx.IsCancellationRequested)
        {
            semaphoreSlim.Release();
            return;
        }

        workerContext.SemaphoreSlim = semaphoreSlim;
        var proxyDequeue = _proxies.TryDequeue(out var proxy);
        while (!proxyDequeue) proxyDequeue = _proxies.TryDequeue(out proxy);
        workerContext.Proxy = proxy;

        var cookies = await wrapper!.GetAuthorizationCookies(
            workerContext.MessageId, workerContext.Account.Login, workerContext.Account.Password, proxy
        );

        if (cookies == null) return;
        var subscription = await wrapper!.GetSubscriptionData(cookies, proxy);

        if (subscription != null && ((subscription.IsSubAccount != null && subscription.IsSubAccount.Value) ||
                                     (subscription.IsTrialUser != null && subscription.IsTrialUser.Value) ||
                                     (subscription.DaysLeft != null && subscription.DaysLeft.Value > 0)))
            workerContext.OnDone(new AccountSuccess(workerContext.Account, true));
        else
            workerContext.OnDone(new AccountSuccess(workerContext.Account, false));

        semaphoreSlim.Release();
    }

    protected override void HandleError(WorkerContext message, Exception exception, Action<Account>? onFail = null)
    {
        if (exception is ChangeProxyException)
        {
            _logger.LogDebug(
                "Try again. {Login}:{Password} {ProxyAddress}:{Port} MessageId = {MessageId}", message.Account.Login,
                message.Account.Password, message.Proxy?.Address, message.Proxy?.Port, message.MessageId
            );

            AddToQueue(new WorkerContext(message.Account, message.OnDone, message.OnFail));
            if (message.Proxy != null) AddProxy(message.Proxy);
        }
        else if (exception is InvalidDataException)
        {
            _logger.LogDebug(
                "Invalid data {Login}:{Password} MessageId = {MessageId}", message.Account.Login, message.Account.Password,
                message.MessageId
            );

            if (message.Proxy != null) AddProxy(message.Proxy);
        }
        else if (exception is UnknownException ex)
        {
            _logger.LogError("{MessageId} | {Message} | {PageSource}", message.MessageId, ex.Message, ex.PageSource);
        }
        else
        {
            base.HandleError(message, exception, onFail);
        }

        message.SemaphoreSlim.Release();
    }
}