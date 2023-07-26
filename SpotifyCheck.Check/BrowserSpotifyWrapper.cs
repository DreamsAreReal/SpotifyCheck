using System.Net;
using Microsoft.Extensions.Options;
using SpotifyCheck.Check.Configurations;
using SpotifyCheck.Core.Models;

namespace SpotifyCheck.Check;

public class BrowserSpotifyWrapper : AbstractSpotifyWrapper
{
    private readonly IBrowserAuthorizationWrapper _browserAuthorizationWrapper;

    public BrowserSpotifyWrapper(
        IBrowserAuthorizationWrapper browserAuthorizationWrapper,
        IOptions<SpotifyOptions> spotifyOptions
    ) : base(spotifyOptions.Value)
    {
        _browserAuthorizationWrapper = browserAuthorizationWrapper;
    }

    public override Task<IReadOnlyCollection<Cookie>?> GetAuthorizationCookies(
        Guid taskId,
        string login,
        string password,
        Proxy? proxy = null
    )
    {
        _browserAuthorizationWrapper.InitBrowser(taskId);
        _browserAuthorizationWrapper.SetProxy(taskId, proxy);
        var result = _browserAuthorizationWrapper.Login(taskId, login, password);
        _browserAuthorizationWrapper.Dispose();
        return Task.FromResult<IReadOnlyCollection<Cookie>?>(result);
    }
}