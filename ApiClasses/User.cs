using Newtonsoft.Json;
namespace OsuScoreStats.ApiClasses;

public class User
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("username")]
    public string Username { get; set; }
    [JsonProperty("country_code")]
    public string CountryCode { get; set; }
    [JsonProperty("country")]
    public Country Country { get; set; }
    [JsonProperty("statistics_rulesets")]
    public Dictionary<string, UserRulesetStatistics> RulesetStatistics;
}
