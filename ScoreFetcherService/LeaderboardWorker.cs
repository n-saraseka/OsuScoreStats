using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.ScoreFetcherService;

public class LeaderboardWorker(IScoreFetcher scoreFetcher) : BackgroundService
{
    private string? _cursor;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var beatmapsetsResponse = await scoreFetcher.GetBeatmapsetsAsync(_cursor, stoppingToken);
            _cursor = beatmapsetsResponse.Cursor;
            var beatmapsets = beatmapsetsResponse.Beatmapsets;
            var beatmaps = new List<APIBeatmap>();
            foreach (var beatmapset in beatmapsets)
                foreach (var beatmap in beatmapset.Beatmaps)
                    if (!beatmaps.Contains(beatmap))
                        beatmaps.Add(beatmap);
            
            var beatmapScoreTasks = new List<Task<BeatmapScores>>();
            foreach (var beatmap in beatmaps)
                beatmapScoreTasks.Add(scoreFetcher.GetBeatmapScoresAsync(beatmap, beatmap.Mode, 0, stoppingToken));
            await Task.WhenAll(beatmapScoreTasks);
        }
    }
}