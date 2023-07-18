using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace SpotifyCheck.Core;

public abstract class AbstractMessageHandler<TMessage, TDoneResult, TFailResult> : IDisposable,
    IAbstractMessageHandler<TMessage, TDoneResult, TFailResult>
    where TMessage : AbstractMessage<TDoneResult, TFailResult> where TDoneResult : class where TFailResult : class
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ILogger<AbstractMessageHandler<TMessage, TDoneResult, TFailResult>> _loggger;
    private readonly ConcurrentQueue<TMessage> _messages;

    protected int DelayMs = 0;

    protected AbstractMessageHandler(
        ILogger<AbstractMessageHandler<TMessage, TDoneResult, TFailResult>> loggger,
        ConcurrentQueue<TMessage> messages,
        CancellationTokenSource cancellationTokenSource
    )
    {
        _loggger = loggger;
        _messages = messages;
        _cancellationTokenSource = cancellationTokenSource;
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
                _loggger.LogTrace("Queue {0} listener started", GetType().Name);

                while (true)
                {
                    var dequeued = _messages.TryDequeue(out var message);
                    if (!dequeued || message == null) continue;
                    if (_cancellationTokenSource.Token.IsCancellationRequested) return;
                    var timer = new Stopwatch();
                    _loggger.LogTrace("Message received {0}. Queue {2}", message.MessageId, GetType().Name);
                    timer.Start();

                    try
                    {
                        await Handle(message, _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        HandleError(message.MessageId, ex);
                    }

                    timer.Stop();

                    _loggger.LogTrace(
                        "Completed message processing {0} at {1} ms. Queue: {2}", message.MessageId, timer.ElapsedMilliseconds,
                        GetType().Name
                    );

                    if (DelayMs != 0) Thread.Sleep(DelayMs);
                }
            }
        );
    }

    protected abstract Task Handle(TMessage message, CancellationToken cancellationToken);

    protected virtual void HandleError(Guid messageId, Exception exception)
    {
        _loggger.LogError(
            "Message {0}, exception type {1}, exception text {2}", messageId, exception.GetType().Name, exception.Message
        );

        if (exception.InnerException != null) HandleError(messageId, exception.InnerException);
    }
}