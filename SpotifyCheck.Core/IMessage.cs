namespace SpotifyCheck.Core;

public interface IMessage<in TDoneResult, in TFailResult>
{
    Guid MessageId { get; }
    Action<TDoneResult> OnDone { get; }
    Action<TFailResult>? OnFail { get; }
}