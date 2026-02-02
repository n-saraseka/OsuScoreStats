using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class UserMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get user data from the API
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated User object (or null)</returns>
    public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        return await userRepository.GetAsync(userId, ct);
    }
    
    /// <summary>
    /// Get users data from the API
    /// </summary>
    /// <param name="userIds">Array containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated User objects</returns>
    public async Task<IEnumerable<User>> GetUsersAsync(int[] userIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        var users = await userRepository
            .GetAll()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);
        
        return users;
    }
    
    /// <summary>
    /// Get scores set by user
    /// </summary>
    /// <param name="userId">user ID</param>
    /// <param name="mode">Gameplay mode (osu, taiko, fruits, mania)</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="page">Scores page (if set to null, method returns all scores)</param>
    /// <param name="amountPerPage">Amount of scores per page</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing up to 100 populated Score objects or all Score objects with this userId</returns>
    public async Task<IEnumerable<Score>> GetUserScoresAsync(
        int userId,
        Mode? mode,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? page,
        int? amountPerPage,
        string? sort = "date",
        bool isDesc = true,
        CancellationToken ct = default)
    {
        var scoresAmount = (amountPerPage == null) ? 100 : Math.Min(100, Math.Max((int)amountPerPage, 0));
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var scoreRepository = new ScoreRepository(dbContext);
        
        var query = scoreRepository.GetAll().AsQueryable();

        query = query.Where(s => s.UserId == userId);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);
        
        if (mandatoryMods?.Length > 0)
            if (optionalMods?.Length > 0)
                query = query.Where(s =>
                    mandatoryMods.All(m => s.ModAcronyms.Contains(m)) &&
                    s.ModAcronyms.All(m => mandatoryMods.Contains(m) || optionalMods.Contains(m)));
            else
                query = query.Where(s =>
                    mandatoryMods.All(m => s.ModAcronyms.Contains(m)) &&
                    s.ModAcronyms.All(m => mandatoryMods.Contains(m)));
        else
            if (optionalMods?.Length > 0)
                query = query.Where(s =>
                    s.ModAcronyms.All(m => optionalMods.Contains(m)));

        switch (sort)
        {
            case "totalScore":
                query = (isDesc) ? query.OrderByDescending(s => s.TotalScore) : query.OrderBy(s => s.TotalScore);
                break;
            case "classicTotalScore":
                query = (isDesc) ? query.OrderByDescending(s => s.ClassicTotalScore) : query.OrderBy(s => s.ClassicTotalScore);
                break;
            case "date":
                query = (isDesc) ? query.OrderByDescending(s => s.Date) : query.OrderBy(s => s.Date);
                break;
            default:
                query = (isDesc) ? query.OrderByDescending(s => s.PP) : query.OrderBy(s => s.PP);
                break;
        }

        if (page != null)
        {
            var pageNumber = (int)page;
            var skip = (pageNumber - 1) * scoresAmount;
            query = query
                .Skip(Math.Max(0, skip))
                .Take(scoresAmount);
        }

        var scores = await query.ToListAsync(ct);
        
        var beatmapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();

        foreach (var beatmapId in beatmapIds)
        {
            var scoresOnThisBeatmap = scores.Where(s => s.BeatmapId == beatmapId).ToList();

            var allScoresOnThisBeatmap = await dbContext.Scores
                .Where(s => s.BeatmapId == beatmapId && s.Mode == mode)
                .OrderByDescending(s => s.TotalScore)
                .ThenBy(s => s.Id)
                .ToListAsync(ct);

            for (int i = 0; i < scoresOnThisBeatmap.Count; i++)
            {
                scoresOnThisBeatmap[i].MapRank = allScoresOnThisBeatmap.IndexOf(allScoresOnThisBeatmap
                    .Find(s => s.Id == scoresOnThisBeatmap[i].Id)) + 1;
            }

            foreach (var score in scores)
            {
                if (score.BeatmapId == beatmapId)
                    score.MapRank = scoresOnThisBeatmap.Find(s => s.Id == score.Id).MapRank;
            }
        }
        
        return scores;
    }

    /// <summary>
    /// Get count of scores set by user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="mode">Gameplay mode (osu, taiko, fruits, mania)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Count of scores set by user</returns>
    public async Task<int> GetUserScoresCountAsync(int userId, Mode? mode, CancellationToken ct)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var scoreRepository = new ScoreRepository(dbContext);
        
        var query = scoreRepository.GetAll().AsQueryable();

        query = query.Where(s => s.UserId == userId);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);

        return await query.CountAsync(ct);
    }
}