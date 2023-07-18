namespace SpotifyCheck.Core;

public interface IMessage<TDoneResult, TFailResult>
    where TDoneResult : class where TFailResult : class
{
    Guid MessageId { get; }
    Action<TDoneResult> Done { get; }
    Action<TFailResult> Fail { get; }
}