using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha.Messages.Request;

public class GettingResultRequest : AbstractMessage<SolvedCaptchaResponse, bool>
{
    public override Action<SolvedCaptchaResponse> OnDone { get; }
    public override Action<bool>? OnFail { get; }
    public string Id { get; }
    public ICaptcha Captcha { get; }
    public DateTime CreatedAt { get; private set; }

    public GettingResultRequest(
        string id,
        DateTime createdAt,
        ICaptcha captcha,
        Action<SolvedCaptchaResponse> onDone,
        Action<bool>? onFail = null
    )
    {
        Id = id;
        CreatedAt = createdAt;
        Captcha = captcha;
        OnDone = onDone;
        OnFail = onFail;
    }

    internal void SetCreatedTime()
    {
        CreatedAt = DateTime.Now;
    }
}