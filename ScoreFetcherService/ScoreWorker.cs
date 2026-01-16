namespace OsuScoreStats.ScoreFetcherService;

public class ScoreWorker(IScoreFetcher scoreFetcher) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var scores = await scoreFetcher.GetScoresAsync(stoppingToken);
            
            // process beatmap and user data first
            var userIds = scores.Select(s => s.UserId).Distinct();
            var beatmapIds = scores.Select(s => s.BeatmapId).Distinct();
            var usersTask = scoreFetcher.ProcessUsersAsync(userIds, stoppingToken);
            var beatmapsTask = scoreFetcher.ProcessBeatmapsAsync(beatmapIds, stoppingToken);
            
            await Task.WhenAll(usersTask, beatmapsTask);
            
            // then can process score data
            Task rankedScoresTask = scoreFetcher.ProcessRankedScoresAsync(scores.Where(s => s.PP != null), stoppingToken);
            Task unrankedScoresTask = scoreFetcher.ProcessUnrankedScoresAsync(scores.Where(s => s.PP == null), stoppingToken);
            await Task.WhenAll(rankedScoresTask, unrankedScoresTask);
            
            // do another run of worker after 45 seconds
            await Task.Delay(45000, stoppingToken);
        }
    }
}