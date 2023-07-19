using Microsoft.Extensions.Logging;
using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Messages.Request;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha;

public class CaptchaMessageHandler : AbstractMessageHandler<AbstractRequest, SolvedCaptchaResponse, bool>
{
    private readonly CaptchaResultMessageHandler _captchaResultMessageHandler;
    private readonly ILogger<CaptchaMessageHandler> _logger;
    private readonly RuCaptchaWrapper _ruCaptchaWrapper;

    public CaptchaMessageHandler(
        ILogger<CaptchaMessageHandler> logger,
        RuCaptchaWrapper ruCaptchaWrapper,
        CaptchaResultMessageHandler captchaResultMessageHandler
    ) : base(logger)
    {
        DelayMs = 200;
        _logger = logger;
        _ruCaptchaWrapper = ruCaptchaWrapper;
        _captchaResultMessageHandler = captchaResultMessageHandler;
    }

    protected override async Task Handle(AbstractRequest message, CancellationToken cancellationToken)
    {
        if (message is not ICaptcha captcha) return;
        var id = await _ruCaptchaWrapper.SendToResolve(captcha);
        _logger.LogTrace("Captcha send to getting results id: {0} messageId: {1}", id, message.MessageId);

        _captchaResultMessageHandler.AddToQueue(
            new GettingResultRequest(id, DateTime.Now, captcha, message.OnDone, message.OnFail)
        );
    }

    protected override void HandleError(Guid messageId, Exception exception)
    {
        base.HandleError(messageId, exception);
    }
}