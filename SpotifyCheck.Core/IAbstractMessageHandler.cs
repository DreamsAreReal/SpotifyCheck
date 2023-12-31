namespace SpotifyCheck.Core;

public interface IAbstractMessageHandler<in TMessage, TDoneResult, TFailResult>
    where TMessage : IMessage<TDoneResult, TFailResult>
{
    void AddToQueue(TMessage message);
}