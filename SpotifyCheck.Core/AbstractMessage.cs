namespace SpotifyCheck.Core;

public abstract class AbstractMessage<TDoneResult, TFailResult> : IMessage<TDoneResult, TFailResult>
{
    public Guid MessageId { get; } = Guid.NewGuid();
    public abstract Action<TDoneResult> OnDone { get; }
    public abstract Action<TFailResult>? OnFail { get; }
}