using Microsoft.Extensions.Logging;
using SpotifyCheck.Models;

namespace SpotifyCheck.DataReaders;

public class AccountDataReader : AbstractDataReader<Account>
{
    public AccountDataReader(ILogger<AccountDataReader> logger) : base(logger)
    {
    }

    protected override (bool, Account) Process(string text)
    {
        var data = text.Split(':');

        if (data.Length != 2)
        {
            Logger.LogDebug("Cant parse {text}", text);
            return (false, null)!;
        }

        return (true, new Account(data[0], data[1]));
    }
}