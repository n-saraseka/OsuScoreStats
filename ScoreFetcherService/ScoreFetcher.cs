using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.Calculators;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
using OsuScoreStats.OsuApi;
namespace OsuScoreStats.ScoreFetcherService;

public class ScoreFetcher(OsuApiService osuApiService, ICalculator scoreCalculator, IDbContextFactory<ScoreDataContext> dbContextFactory) : IScoreFetcher
{
    /// <summary>
    /// Get beatmapsets from the API search endpoint
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated BeatmapsetsResponse object</returns>
    public async Task<BeatmapsetsResponse> GetBeatmapsetsAsync(string? cursor, CancellationToken ct = default)
    {
        var beatmapsets = await osuApiService.GetBeatmapsetsAsync(cursor, ct);
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        BeatmapsetRepository beatmapsetRepository = new(dbContext);
        await beatmapsetRepository.CreateBulkAsync(beatmapsets.Beatmapsets, ct);
        
        return beatmapsets;
    }

    public async Task<BeatmapScores> GetBeatmapScoresAsync(APIBeatmap beatmap, Mode? mode, int legacyOnly = 0, CancellationToken ct = default)
    {
        var scores = await osuApiService.GetBeatmapScoresAsync(beatmap.Id, mode, legacyOnly, ct); 
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        BeatmapRepository beatmapRepository = new(dbContext);
        await beatmapRepository.CreateAsync(beatmap, ct);
        
        var users = scores.Scores.Select(s => s.User).Distinct().ToList();
        UserRepository userRepository = new(dbContext);
        await userRepository.CreateBulkAsync(users, ct);
        
        ScoreRepository scoreRepository = new(dbContext);
        ProcessModAcronyms(scores.Scores);

        var unrankedScores = scores.Scores.Where(s => s.PP == null);
        var rankedScores =  scores.Scores.Where(s => s.PP != null);

        var scoreTasks = new List<Task>();
        
        scoreTasks.Add(ProcessRankedScoresAsync(rankedScores, ct));
        if (unrankedScores.Count() > 0)
            scoreTasks.Add(ProcessUnrankedScoresAsync(unrankedScores, ct));
        
        await Task.WhenAll(scoreTasks);

        return scores;
    }
    
    /// <summary>
    /// Get scores from the API firehose
    /// </summary>
    /// <param name="cursor">Cursor string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated ScoresResponse object</returns>
    public async Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct = default)
    {
        var scores = await osuApiService.GetScoresAsync(cursor, ct);
        ProcessModAcronyms(scores.Scores);
        return scores;
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
        var end = scoresList[scoresList.Count - 1].Date;
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
        var end = scoresList[scoresList.Count - 1].Date;
        var scoresCounter = scoresList.Count;
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        
        var scoreRepository = new ScoreRepository(dbContext);
        await scoreRepository.CreateBulkAsync(scoresList, ct);
        
        Console.WriteLine($"Saved {scoresCounter} ranked scores between {start} and {end} to the DB.");
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
        var existingUserIds = await userRepository
            .GetAll()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.Id)
            .ToListAsync(ct);
        
        // update old users first
        var usersToUpdate = users.Where(u => existingUserIds.Contains(u.Id)).ToList();
        await userRepository.UpdateBulkAsync(usersToUpdate, ct);
        
        // then create new users
        var newUsers = users.Where(u => !existingUserIds.Contains(u.Id)).ToList();
        await userRepository.CreateBulkAsync(newUsers, ct);
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
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        var existingBeatmapIds = await beatmapRepository
            .GetAll()
            .Where(b => beatmapIds.Contains(b.Id))
            .Select(b => b.Id)
            .ToListAsync(ct);
        
        var newBeatmapIds = beatmapIds.Where(id => !existingBeatmapIds.Contains(id));
        
        if (newBeatmapIds.Count() > 0)
        {
            for (int i = 0; i < newBeatmapIds.Count(); i += batchSize)
            {
                var batch = newBeatmapIds.Skip(i).Take(batchSize).ToList();
                APIBeatmap[] beatmapData = await osuApiService.GetBeatmapsAsync(batch, ct);
                beatmaps.AddRange(beatmapData);
            }
        }

        // create new maps
        await beatmapRepository.CreateBulkAsync(beatmaps, ct);
    }

    private void ProcessModAcronyms(IEnumerable<Score> scores)
    {
        foreach (var score in scores)
        {
            var modAcronyms = new List<string>();
            foreach (var mod in score.Mods)
            {
                var acronym = mod.Acronym;
                if (mod.Settings.ContainsKey("speed_change"))
                    acronym += $"({mod.Settings["speed_change"]}x)";
                modAcronyms.Add(acronym);
            }
                
            score.ModAcronyms = modAcronyms.ToArray();
        }
    }
}