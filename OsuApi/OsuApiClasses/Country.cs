using Newtonsoft.Json;
namespace OsuScoreStats.OsuApi.OsuApiClasses;

public class Country
{
    [JsonProperty("code")]
    public string Code { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}
