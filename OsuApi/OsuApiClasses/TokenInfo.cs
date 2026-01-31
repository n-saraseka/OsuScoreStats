using Newtonsoft.Json;
namespace OsuScoreStats.OsuApi.OsuApiClasses;

public class TokenInfo
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("expires_in")]
    public long ExpiresIn { get; set; }
}