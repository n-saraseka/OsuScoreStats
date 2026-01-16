using Newtonsoft.Json;
namespace OsuScoreStats.ApiClasses;

public class UserRulesetStatistics
{
    [JsonProperty("global_rank")]
    public int? GlobalRank { get; set; }
    [JsonProperty("pp")]
    public float PP { get; set; } = 0;
}
