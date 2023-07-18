using SpotifyCheck.Core;
using SpotifyCheck.RuCaptcha.Messages.Responses;

namespace SpotifyCheck.RuCaptcha.Messages.Request;

public abstract class AbstractRequest : AbstractMessage<SolvedCaptcha, bool>
{
}