using System.Net;
using System.Text.Json;
using SpotifyCheck.Check.Configurations;
using SpotifyCheck.Check.Models;
using SpotifyCheck.Core.Models;

namespace SpotifyCheck.Check;

public abstract class AbstractSpotifyWrapper : IDisposable
{
    private readonly SpotifyOptions _spotifyOptions;

    public AbstractSpotifyWrapper(SpotifyOptions spotifyOptions)
    {
        _spotifyOptions = spotifyOptions;
    }

    public abstract void Dispose();

    public abstract Task<IReadOnlyCollection<Cookie>?> GetAuthorizationCookies(
        Guid taskId,
        string login,
        string password,
        Proxy? proxy = null
    );

    public async Task<Subscription?> GetSubscriptionData(IReadOnlyCollection<Cookie> cookies, Proxy? proxy = null)
    {
        HttpClientHandler httpHandler = new HttpClientHandler
        {
            AllowAutoRedirect = true, UseCookies = true, CookieContainer = new CookieContainer()
        };

        if (proxy != null)
        {
            WebProxy webProxy = new WebProxy($"{proxy.GetTypeNameForHttpClient()}://{proxy.Address}:{proxy.Port}", false);

            if (!string.IsNullOrWhiteSpace(proxy.Login) && !string.IsNullOrWhiteSpace(proxy.Password))
                webProxy.Credentials = new NetworkCredential(proxy.Login, proxy.Password);

            httpHandler.Proxy = webProxy;
            httpHandler.UseProxy = true;
        }

        foreach (Cookie cookie in cookies)
            httpHandler.CookieContainer.Add(cookie);

        HttpClient httpClient = new HttpClient(httpHandler);
        HttpResponseMessage response = await httpClient.GetAsync(_spotifyOptions.GetSubscriptionDataUrl);
        string responseText = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Subscription>(responseText);
    }
}