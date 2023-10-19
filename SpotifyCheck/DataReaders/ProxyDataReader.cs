using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SpotifyCheck.Core.Models;

namespace SpotifyCheck.DataReaders;

public class ProxyDataReader : AbstractDataReader<Proxy>
{
    public ProxyDataReader(ILogger<ProxyDataReader> logger) : base(logger)
    {
    }

    protected override (bool, Proxy) Process(string text)
    {
        Match regex = new Regex("(.*):\\/\\/(.*):(.*)@(.*):(.*)").Match(text);
        bool typeParsed = Enum.TryParse(regex.Groups[1].Value, true, out ProxyType type);

        if (!typeParsed)
        {
            Logger.LogDebug("Cant parse {text}", text);
            return (false, null)!;
        }

        return (true,
            new Proxy(regex.Groups[4].Value, regex.Groups[5].Value, regex.Groups[2].Value, regex.Groups[3].Value, type));
    }
}