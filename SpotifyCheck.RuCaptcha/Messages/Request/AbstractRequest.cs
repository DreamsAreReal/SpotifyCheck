using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Exceptions;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha.Messages.Request;

public abstract class AbstractRequest : AbstractMessage<SolvedCaptchaResponse, ErrorType>
{
    public override Action<SolvedCaptchaResponse> OnDone { get; }
    public override Action<ErrorType>? OnFail { get; }

    protected AbstractRequest(Action<SolvedCaptchaResponse> onDone, Action<ErrorType> onFail)
    {
        OnDone = onDone;
        OnFail = onFail;
    }
}