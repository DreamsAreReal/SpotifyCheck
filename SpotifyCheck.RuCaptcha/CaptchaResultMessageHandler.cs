using Microsoft.Extensions.Logging;
using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Messages.Request;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha;

public class CaptchaResultMessageHandler : AbstractMessageHandler<GettingResultRequest, SolvedCaptchaResponse, bool>
{
    private readonly ILogger<CaptchaResultMessageHandler> _logger;
    private readonly RuCaptchaWrapper _ruCaptchaWrapper;

    public CaptchaResultMessageHandler(ILogger<CaptchaResultMessageHandler> logger, RuCaptchaWrapper ruCaptchaWrapper) : base(
        logger
    )
    {
        DelayMs = 250;
        _logger = logger;
        _ruCaptchaWrapper = ruCaptchaWrapper;
    }

    protected override async Task Handle(GettingResultRequest message, CancellationToken cancellationToken)
    {
        var pastTimeMs = DateTime.Now.Subtract(message.CreatedAt);

        if (DateTime.Now.Subtract(message.CreatedAt) < TimeSpan.FromMilliseconds(message.Captcha.DelayToGetResultMs))
            Thread.Sleep(TimeSpan.FromMilliseconds(message.Captcha.DelayToGetResultMs).Subtract(pastTimeMs));

        var answer = await _ruCaptchaWrapper.GetResult(message.Id);

        if (!string.IsNullOrWhiteSpace(answer))
        {
            _logger.LogTrace("Captcha has resolved id: {0} messageId: {1}", message.Id, message.MessageId);
            message.OnDone(new SolvedCaptchaResponse(message.Id, answer));
            return;
        }

        message.SetCreatedTime();
        AddToQueue(message);
    }

    protected override void HandleError(Guid messageId, Exception exception)
    {
        base.HandleError(messageId, exception);
    }
}