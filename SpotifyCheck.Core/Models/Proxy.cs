namespace SpotifyCheck.Core.Models;

public record Proxy(string Address, string Port, string Login, string Password, ProxyType Type)
{
    public string GetTypeNameForHttpClient()
    {
        switch (Type)
        {
            case ProxyType.HTTP: return "http";
            case ProxyType.HTTPS: return "http";
            case ProxyType.SOCKS4: return "socks4";
            case ProxyType.SOCKS5: return "socks5";
        }

        throw new Exception("Unknown type");
    }
}