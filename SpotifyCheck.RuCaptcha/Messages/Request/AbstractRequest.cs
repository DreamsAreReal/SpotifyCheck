using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha.Messages.Request;

public abstract class AbstractRequest : AbstractMessage<SolvedCaptchaResponse, bool>
{
    public override Action<SolvedCaptchaResponse> OnDone { get; }
    public override Action<bool>? OnFail { get; }

    protected AbstractRequest(Action<SolvedCaptchaResponse> onDone, Action<bool>? onFail = null)
    {
        OnDone = onDone;
        OnFail = onFail;
    }
}