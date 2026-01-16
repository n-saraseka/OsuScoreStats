using Newtonsoft.Json;
namespace OsuScoreStats.ApiClasses;

public class ScoresResponse
{
    [JsonProperty("scores")]
    public Score[] Scores { get; set; }
    [JsonProperty("cursor_string")]
    public string Cursor { get; set; }
}
