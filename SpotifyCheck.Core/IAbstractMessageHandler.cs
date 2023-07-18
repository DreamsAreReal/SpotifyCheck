namespace SpotifyCheck.Core;

public interface IAbstractMessageHandler<TMessage, TDoneResult, TFailResult>
    where TMessage : IMessage<TDoneResult, TFailResult> where TDoneResult : class where TFailResult : class
{
    void AddToQueue(TMessage message);
}