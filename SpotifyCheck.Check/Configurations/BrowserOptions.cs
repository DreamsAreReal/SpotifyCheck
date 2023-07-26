namespace SpotifyCheck.Check.Configurations;

public class BrowserOptions
{
    public string ProxyExtensionPath { get; set; } = string.Empty;
    public bool LaunchHeadless { get; set; } = true;

    public int GetProxyExtensionUrlTimeoutMs { get; set; }
    public int GetProxyExtensionWindowsTimeoutMs { get; set; }
    public int GetProxyFormTimeoutMs { get; set; }
    public int GetProxyLoaderTimeoutMs { get; set; }
    public int GetProxySelectTimeoutMs { get; set; }

    public string SpotifyUrl { get; set; } = string.Empty;

    public int GetSpotifyFormTimeoutMs { get; set; }
    public int GetSpotifyErrorMessageMs { get; set; }
    public int GetSpotifyCookieTimeoutMs { get; set; }
}