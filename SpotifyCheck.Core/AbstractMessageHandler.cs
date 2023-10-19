using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SpotifyCheck.Core;

public abstract class AbstractMessageHandler<TMessage, TDoneResult, TFailResult> : IDisposable,
    IAbstractMessageHandler<TMessage, TDoneResult, TFailResult>
    where TMessage : AbstractMessage<TDoneResult, TFailResult>
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<AbstractMessageHandler<TMessage, TDoneResult, TFailResult>> _logger;
    private readonly ConcurrentQueue<TMessage> _messages;

    protected int DelayMs = 0;

    protected AbstractMessageHandler(ILogger<AbstractMessageHandler<TMessage, TDoneResult, TFailResult>> logger)
    {
        _logger = logger;
        _messages = new ConcurrentQueue<TMessage>();
        _cancellationTokenSource = new CancellationTokenSource();
        GetListenerThread().Start();
    }

    public void AddToQueue(TMessage message)
    {
        _messages.Enqueue(message);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Dispose();
    }

    private Thread GetListenerThread()
    {
        return new Thread(
            async () =>
            {
                _logger.LogTrace("Queue {ListenerName} listener started", GetType().Name);

                while (true)
                {
                    bool dequeued = _messages.TryDequeue(out TMessage? message);

                    if (!dequeued || message == null)
                        continue;

                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        return;

                    Stopwatch timer = new();
                    _logger.LogTrace("Message received {MessageId}. Queue {ListenerName}", message.MessageId, GetType().Name);
                    timer.Start();

                    try
                    {
                        await Handle(message, _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        HandleError(message, ex, message.OnFail);
                    }

                    timer.Stop();

                    _logger.LogTrace(
                        "Completed message processing {MessageId} at {ElapsedMilliseconds} ms. Queue: {ListenerName}",
                        message.MessageId, timer.ElapsedMilliseconds, GetType().Name
                    );

                    if (DelayMs != 0)
                        Thread.Sleep(DelayMs);
                }
            }
        );
    }

    protected abstract Task Handle(TMessage message, CancellationToken cancellationToken);

    protected virtual void HandleError(TMessage message, Exception exception, Action<TFailResult>? onFail = null)
    {
        _logger.LogError(
            "Message {MessageId}, exception type {ExceptionType}, exception text {ExceptionText}", message.MessageId,
            exception.GetType().Name, exception.Message
        );

        if (exception.InnerException != null)
            HandleError(message, exception.InnerException, onFail);
    }
}