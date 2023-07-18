using Microsoft.Extensions.Logging;
using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Messages.Request;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha;

public class RuCaptchaMessageHandler : AbstractMessageHandler<ReCaptchaV3, SolvedCaptcha, bool>
{
    private readonly ILogger<AbstractMessageHandler<ReCaptchaV3, SolvedCaptcha, bool>> _logger;

    public RuCaptchaMessageHandler(ILogger<RuCaptchaMessageHandler> logger) : base(logger)
    {
        DelayMs = 100;
        _logger = logger;
    }

    protected override Task Handle(ReCaptchaV3 message, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override void HandleError(Guid messageId, Exception exception)
    {
        base.HandleError(messageId, exception);
    }
}