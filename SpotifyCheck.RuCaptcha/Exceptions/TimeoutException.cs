namespace SpotifyCheck.RuCaptcha.Exceptions;

public class TimeoutException : Exception
{
    public int DelayMs { get; }

    public TimeoutException(int delayMs) : base($"Need timeout {delayMs} ms")
    {
        DelayMs = delayMs;
    }
}