using Newtonsoft.Json;
namespace OsuScoreStats.ApiClasses;

public class Beatmapset
{
    [JsonProperty("id")]
    public int Id { get; set; }
    [JsonProperty("artist")]
    public string Artist { get; set; }
    [JsonProperty("title")]
    public string Title { get; set; }
    [JsonProperty("preview_url")]
    public string PreviewUrl { get; set; }
}