using Microsoft.Extensions.Options;
using SpotifyCheck.RuCaptcha.Configurations;
using SpotifyCheck.RuCaptcha.Exceptions;
using SpotifyCheck.RuCaptcha.Messages.Request;

namespace SpotifyCheck.RuCaptcha;

public class RuCaptchaWrapper : IDisposable
{
    private const string _inboxUrl = "http://rucaptcha.com/in.php";
    private const string _outboxUrl = "http://rucaptcha.com/res.php";

    private readonly Dictionary<string, string> _additionalParameters;
    private readonly string _additionalQuery;
    private readonly HttpClient _client;
    private readonly RuCaptchaErrorCodes _ruCaptchaErrorCodes;

    public RuCaptchaWrapper(IOptions<RuCaptchaOptions> ruCaptchaOptions, RuCaptchaErrorCodes ruCaptchaErrorCodes)
    {
        _client = new HttpClient();
        _additionalParameters = new Dictionary<string, string> { { "key", ruCaptchaOptions.Value.Key } };
        _additionalQuery = $"&key={ruCaptchaOptions.Value.Key}";
        _ruCaptchaErrorCodes = ruCaptchaErrorCodes;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task<string> SendToResolve(ICaptcha captcha)
    {
        HttpResponseMessage response = await _client.PostAsync(
            _inboxUrl, new FormUrlEncodedContent(captcha.ToDictionary().Union(_additionalParameters))
        );

        string result = await response.Content.ReadAsStringAsync();
        _ruCaptchaErrorCodes.ThrowExceptionIfRecognizeError(result);
        string[] answerTmp = result.Split('|');

        if (answerTmp.Length < 2)
            throw new Exception(result);

        return answerTmp[1];
    }

    public async Task<string?> GetResult(string id)
    {
        HttpResponseMessage response = await _client.GetAsync($"{_outboxUrl}?action=get&id={id}{_additionalQuery}");
        string result = await response.Content.ReadAsStringAsync();
        _ruCaptchaErrorCodes.ThrowExceptionIfRecognizeError(result);

        if (result.Contains(RuCaptchaErrorCodes.CaptchaNotReady))
            return null;

        string[] answerTmp = result.Split('|');

        if (answerTmp.Length < 2)
            throw new Exception(result);

        return answerTmp[1];
    }
}