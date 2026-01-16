using Microsoft.EntityFrameworkCore;
using OsuScoreStats.ApiClasses;
using OsuScoreStats.Calculators;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ScoreFetcherService;

public class ScoreFetcher(OsuApiService osuApiService, ICalculator scoreCalculator, IDbContextFactory<ScoreDataContext> dbContextFactory) : IScoreFetcher
{
    public async Task<IEnumerable<Score>> GetScoresAsync(CancellationToken ct = default)
    {
        return await osuApiService.GetScoresAsync(ct);
    }
    /// <summary>
    /// Process data from unranked scores, including PP calculation. Calculates highest PP scores for each mode
    /// </summary>
    /// <param name="scores">Unranked scores to process</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task ProcessUnrankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var scoresList = scores.ToList();
        
        var start = scoresList[0].Date;
        var end = scoresList[-1].Date;
        var scoresCounter = scoresList.Count;

        for (int i = 0; i < scoresCounter; i++)
            scoresList[i].PP = await scoreCalculator.CalculateAsync(scoresList[i], ct);

        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        ScoreRepository scoreRepository = new(dbContext);
        await scoreRepository.CreateBulkAsync(scoresList, ct);
        
        Console.WriteLine($"Saved {scoresCounter} unranked scores between {start} and {end} to the DB.");
    }
    
    /// <summary>
    /// Process data from ranked scores. Calculates highest PP scores for each mode
    /// </summary>
    /// <param name="scores">Ranked scores to process</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task ProcessRankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var scoresList = scores.ToList();
        
        var start = scoresList[0].Date;
        var end = scoresList[-1].Date;
        var scoresCounter = scoresList.Count;
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var scoreRepository = new ScoreRepository(dbContext);
        await scoreRepository.CreateBulkAsync(scoresList, ct);
        
        Console.WriteLine($"Saved {scoresCounter} unranked scores between {start} and {end} to the DB.");
    }
    
    /// <summary>
    /// Process user data from user IDs
    /// </summary>
    /// <param name="userIds">IEnumerable containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task ProcessUsersAsync(IEnumerable<int> userIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var users = new List<User>();
        
        if (userIds.Count() > 0)
        {
            for (int i = 0; i < userIds.Count(); i += batchSize)
            {
                var batch = userIds.Skip(i).Take(batchSize).ToList();
                User[] userData = await osuApiService.GetUsersAsync(batch, ct);
                users.AddRange(userData);
            }
        }
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var userRepository = new UserRepository(dbContext);
        await userRepository.CreateBulkAsync(users, ct);
    }
    
    /// <summary>
    /// Process beatmap data from beatmap IDs
    /// </summary>
    /// <param name="beatmapIds">IEnumerable containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task ProcessBeatmapsAsync(IEnumerable<int> beatmapIds, CancellationToken ct = default)
    {
        const int batchSize = 50;
        var beatmaps = new List<APIBeatmap>();
        
        if (beatmapIds.Count() > 0)
        {
            for (int i = 0; i < beatmapIds.Count(); i += batchSize)
            {
                var batch = beatmapIds.Skip(i).Take(batchSize).ToList();
                APIBeatmap[] beatmapData = await osuApiService.GetBeatmapsAsync(batch, ct);
                beatmaps.AddRange(beatmapData);
            }
        }
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);

        var beatmapRepository = new BeatmapRepository(dbContext);
        await beatmapRepository.CreateBulkAsync(beatmaps, ct);
    }
}