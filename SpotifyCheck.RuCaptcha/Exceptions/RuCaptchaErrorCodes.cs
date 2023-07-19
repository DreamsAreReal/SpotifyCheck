namespace SpotifyCheck.RuCaptcha.Exceptions;

public class RuCaptchaErrorCodes
{
    public const string CaptchaNotReady = "CAPCHA_NOT_READY";
    private const string _captchaCantRecognize = "ERROR_CAPTCHA_UNSOLVABLE";

    private readonly List<string> _needChangeProxy = new() { "ERROR_PROXY_CONNECTION_FAILED", "ERROR_BAD_PROXY" };

    private readonly Dictionary<string, int> _notCriticalWithTimeouts = new()
    {
        { "ERROR_NO_SLOT_AVAILABLE", 5 * 1000 },
        { "IP_BANNED", 60 * 6 * 1000 },
        { "MAX_USER_TURN", 2 * 1000 },
        { "ERROR:", 11 * 60 * 1000 }
    };

    internal void ThrowExceptionIfRecognizeError(string response)
    {
        if (response.Contains(_captchaCantRecognize)) throw new CaptchaNotRecognizedException();

        if (_needChangeProxy.Any(x => response.Contains(x)))
        {
            throw new ChangeProxyException();
        }

        if (_notCriticalWithTimeouts.Any(x => response.Contains(x.Key)))
        {
            var timeout = _notCriticalWithTimeouts.Where(x => response.Contains(x.Key)).Select(x => x.Value).FirstOrDefault();
            throw new TimeoutException(timeout);
        }
    }
}