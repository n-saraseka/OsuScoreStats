using Newtonsoft.Json;
namespace OsuScoreStats.OsuApi.OsuApiClasses;

public class BeatmapScores
{
    [JsonProperty("scores")]
    public Score[] Scores { get; set; } = Array.Empty<Score>();
}