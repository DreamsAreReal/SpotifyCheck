using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpotifyCheck.RuCaptcha.Configuration;
using SpotifyCheck.RuCaptcha.Messages.Request;

namespace SpotifyCheck.RuCaptcha;

public class RuCaptchaWrapper : IDisposable
{
    private const string _inboxUrl = "http://rucaptcha.com/in.php";
    private const string _outboxUrl = "http://rucaptcha.com/res.php";

    private readonly Dictionary<string, string> _additionalParameters;
    private readonly HttpClient _client;
    private string _additionalQuery;

    private ILogger<RuCaptchaWrapper> _logger;

    public RuCaptchaWrapper(ILogger<RuCaptchaWrapper> logger, IOptions<RuCaptchaOptions> ruCaptchaOptions)
    {
        _client = new HttpClient();
        _additionalParameters = new Dictionary<string, string> { { "key", ruCaptchaOptions.Value.Key }, { "json", "1" } };
        _additionalQuery = $"&key={ruCaptchaOptions.Value.Key}";
        _logger = logger;
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public async Task<string> SendToResolve(ICaptcha captcha)
    {
        var response = await _client.PostAsync(
            _inboxUrl, new FormUrlEncodedContent(captcha.ToDictionary().Union(_additionalParameters))
        );

        var id = await response.Content.ReadAsStringAsync();
        return id;
    }
}