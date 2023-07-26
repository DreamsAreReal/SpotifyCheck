using SpotifyCheck.Core;
using SpotifyCheck.Core.Models;

namespace SpotifyCheck.Models;

public class WorkerContext : AbstractMessage<AccountSuccess, AccountError>
{
    public Account Account { get; }
    public Proxy? Proxy { get; set; }
    public override Action<AccountSuccess> OnDone { get; }
    public override Action<AccountError>? OnFail { get; }

    public WorkerContext(Account account, Action<AccountSuccess> onDone, Action<AccountError>? onFail)
    {
        Account = account;
        OnDone = onDone;
        OnFail = onFail;
    }
}