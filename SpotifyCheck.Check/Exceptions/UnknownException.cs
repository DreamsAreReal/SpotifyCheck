namespace SpotifyCheck.Check.Exceptions;

public class UnknownException : Exception
{
    public string PageSource { get; private set; }

    public UnknownException(string pageSource, string message) : base(message)
    {
        PageSource = pageSource;
    }
}