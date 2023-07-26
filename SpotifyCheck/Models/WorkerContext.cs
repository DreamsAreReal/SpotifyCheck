using SpotifyCheck.Core;
using SpotifyCheck.Core.Models;

namespace SpotifyCheck.Models;

public class WorkerContext : AbstractMessage<AccountSuccess, Account>
{
    public Account Account { get; }
    public Proxy? Proxy { get; set; }
    public SemaphoreSlim SemaphoreSlim { get; set; }
    public override Action<AccountSuccess> OnDone { get; }
    public override Action<Account>? OnFail { get; }

    public WorkerContext(Account account, Action<AccountSuccess> onDone, Action<Account>? onFail)
    {
        Account = account;
        OnDone = onDone;
        OnFail = onFail;
    }
}