using System.Text.Json.Serialization;

namespace SpotifyCheck.Check.Models;

public record Subscription(
    [property: JsonPropertyName("isTrialUser")]
    bool? IsTrialUser,
    [property: JsonPropertyName("currentPlan")]
    string CurrentPlan,
    [property: JsonPropertyName("isRecurring")]
    bool? IsRecurring,
    [property: JsonPropertyName("daysLeft")]
    int? DaysLeft,
    [property: JsonPropertyName("accountAgeDays")]
    int? AccountAgeDays,
    [property: JsonPropertyName("isSubAccount")]
    bool? IsSubAccount
);