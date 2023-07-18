namespace SpotifyCheck.RuCaptcha.Messages.Request;

public interface ICaptcha
{
    int DelayToGetResultMs { get; }
    Dictionary<string, string> ToDictionary();
}