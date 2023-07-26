using SpotifyCheck.Core.Models;
using SpotifyCheck.RuCaptcha.Exceptions;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha.Messages.Request;

public class ReCaptchaV3Request : AbstractRequest, ICaptcha
{
    public string Method { get; } = "userrecaptcha";
    public string Version { get; } = "v3";
    public string SiteKey { get; init; }
    public string PageUrl { get; init; }
    public bool IsEnterprise { get; init; }

    public Proxy? Proxy { get; init; }

    public ReCaptchaV3Request(
        Action<SolvedCaptchaResponse> onDone,
        Action<ErrorType> onFail,
        string siteKey,
        string pageUrl,
        bool isEnterprise,
        Proxy? proxy = null
    ) : base(onDone, onFail)
    {
        SiteKey = siteKey;
        PageUrl = pageUrl;
        IsEnterprise = isEnterprise;
        Proxy = proxy;
    }

    public int DelayToGetResultMs { get; } = 20 * 1000;

    public Dictionary<string, string> ToDictionary()
    {
        var parameters = new Dictionary<string, string>
        {
            { "method", Method },
            { "version", Version },
            { "enterprise", IsEnterprise ? "1" : "0" },
            { "googlekey", SiteKey },
            { "pageurl", PageUrl }
        };

        if (Proxy != null)
        {
            parameters.Add("proxy", $"{Proxy.Login}:{Proxy.Password}@{Proxy.Address}:{Proxy.Port}");
            parameters.Add("proxytype", Proxy.Type.ToString());
        }

        return parameters;
    }
}