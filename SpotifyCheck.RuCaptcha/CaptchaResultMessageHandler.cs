using Microsoft.Extensions.Logging;
using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Exceptions;
using SpotifyCheck.RuCaptcha.Messages.Request;
using SpotifyCheck.RuCaptcha.Messages.Responses;
using TimeoutException = SpotifyCheck.RuCaptcha.Exceptions.TimeoutException;

namespace SpotifyCheck.RuCaptcha;

public class CaptchaResultMessageHandler : AbstractMessageHandler<GettingResultRequest, SolvedCaptchaResponse, ErrorType>
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
        TimeSpan pastTimeMs = DateTime.Now.Subtract(message.CreatedAt);

        if (DateTime.Now.Subtract(message.CreatedAt) < TimeSpan.FromMilliseconds(message.Captcha.DelayToGetResultMs))
            Thread.Sleep(TimeSpan.FromMilliseconds(message.Captcha.DelayToGetResultMs).Subtract(pastTimeMs));

        string? answer = await _ruCaptchaWrapper.GetResult(message.Id);

        if (!string.IsNullOrWhiteSpace(answer))
        {
            _logger.LogTrace("Captcha has resolved id: {CaptchaId} messageId: {MessageId}", message.Id, message.MessageId);
            message.OnDone(new SolvedCaptchaResponse(message.Id, answer));
            return;
        }

        message.SetCreatedTime();
        AddToQueue(message);
    }

    protected override void HandleError(GettingResultRequest message, Exception exception, Action<ErrorType>? onFail = null)
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