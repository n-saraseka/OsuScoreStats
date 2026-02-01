using Newtonsoft.Json;
namespace OsuScoreStats.OsuApi.OsuApiClasses;

public class BeatmapsetsResponse
{
    [JsonProperty("beatmapsets")]
    public Beatmapset[] Beatmapsets { get; set; } = Array.Empty<Beatmapset>();
    [JsonProperty("cursor_string")]
    public string CursorString { get; set; } = string.Empty;
}