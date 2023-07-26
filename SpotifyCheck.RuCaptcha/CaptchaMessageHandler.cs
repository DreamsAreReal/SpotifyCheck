using Microsoft.Extensions.Logging;
using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Exceptions;
using SpotifyCheck.RuCaptcha.Messages.Request;
using SpotifyCheck.RuCaptcha.Messages.Responses;
using TimeoutException = SpotifyCheck.RuCaptcha.Exceptions.TimeoutException;

namespace SpotifyCheck.RuCaptcha;

public class CaptchaMessageHandler : AbstractMessageHandler<AbstractRequest, SolvedCaptchaResponse, ErrorType>
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
        _logger.LogTrace("Captcha send to getting results id: {CaptchaId} messageId: {MessageId}", id, message.MessageId);

        _captchaResultMessageHandler.AddToQueue(
            new GettingResultRequest(id, DateTime.Now, captcha, message.OnDone, message.OnFail)
        );
    }

    protected override void HandleError(AbstractRequest message, Exception exception, Action<ErrorType>? onFail = null)
    {
        if (exception is TimeoutException timeoutException)
        {
            _logger.LogTrace(
                "RuCaptcha requests block at {Delay} ms. Message id: {MessageId}", timeoutException.DelayMs, message.MessageId
            );

            Thread.Sleep(timeoutException.DelayMs);
            onFail?.Invoke(ErrorType.TryAgain);
            return;
        }

        if (exception is ChangeProxyException changeProxyException)
        {
            _logger.LogTrace("RuCaptcha cant use proxy. Message id: {MessageId}", message.MessageId);
            onFail?.Invoke(ErrorType.ChangeProxy);
        }
        else if (exception is CaptchaNotRecognizedException captchaNotRecognizedException)
        {
            _logger.LogTrace("RuCaptcha cant recognize captcha. Message id: {MessageId}", message.MessageId);
            onFail?.Invoke(ErrorType.TryAgain);
        }
        else
        {
            base.HandleError(message, exception, onFail);
        }

        onFail?.Invoke(ErrorType.Critical);
    }
}