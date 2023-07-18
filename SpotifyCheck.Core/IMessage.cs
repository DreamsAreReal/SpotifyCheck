namespace SpotifyCheck.Core;

public interface IMessage<TDoneResult, TFailResult>
{
    Guid MessageId { get; }
    Action<TDoneResult> Done { get; }
    Action<TFailResult> Fail { get; }
}