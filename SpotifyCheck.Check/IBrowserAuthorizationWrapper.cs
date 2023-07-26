using System.Collections.ObjectModel;
using System.Net;
using SpotifyCheck.Core.Models;

namespace SpotifyCheck.Check;

public interface IBrowserAuthorizationWrapper : IDisposable
{
    void InitBrowser(Guid taskId);
    void SetProxy(Guid taskId, Proxy? proxy);
    ReadOnlyCollection<Cookie>? Login(Guid taskId, string login, string password);
}