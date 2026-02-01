using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class ScoreMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get scores
    /// </summary>
    /// <param name="mode">Gameplay mode (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="dateStart">Date to begin getting scores from (defaults to latest date in scores table)</param>
    /// <param name="dateEnd">Date to end getting scores from (defaults to latest date in scores table)</param>
    /// <param name="country">Country code</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="sort">Parameter to sort by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing up to 100 highest pp scores at given date</returns>
    public async Task<IEnumerable<Score>> GetScoresAsync(
        Mode? mode, 
        DateOnly? dateStart,
        DateOnly? dateEnd,
        string? country,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount = 25,
        string? sort = "pp",
        bool isDesc = true,
        CancellationToken ct = default)
    {
        var scoresAmount = (amount == null) ? 25 : Math.Min(100, Math.Max((int)amount, 0));
        
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var scoreRepository = new ScoreRepository(dbContext);
        
        var query = scoreRepository.GetAll().AsQueryable();
        
        var latestDate = await query.MaxAsync(s => s.Date, ct);
        var targetStartDate = dateStart ?? DateOnly.FromDateTime(latestDate);
        var targetEndDate = dateEnd ?? DateOnly.FromDateTime(latestDate);
        query = query.Where(s => 
            DateOnly.FromDateTime(s.Date) >= targetStartDate && DateOnly.FromDateTime(s.Date) <= targetEndDate);
        
        if (mode.HasValue)
            query = query.Where(s => s.Mode == mode.Value);
        
        if (country != null)
        {
            var userRepository = new UserRepository(dbContext);
            var userIdsThisCountry = await userRepository
                .GetAll()
                .Where(u => u.CountryCode == country)
                .Select(u => u.Id).Distinct()
                .ToListAsync(ct);
            query = query.Where(s => userIdsThisCountry.Contains(s.UserId));
        }
        
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

        var recentScores = new List<Score>();

        if (isDesc)
        {
            switch (sort)
            {
                case "totalScore":
                    recentScores = await query.OrderByDescending(s => s.TotalScore).Take(scoresAmount).ToListAsync(ct);
                    break;
                case "date":
                    recentScores = await query.OrderByDescending(s => s.Date).Take(scoresAmount).ToListAsync(ct);
                    break;
                default:
                    recentScores = await query.OrderByDescending(s => s.PP).Take(scoresAmount).ToListAsync(ct);
                    break;
            }
        }
        
        else
        {
            switch (sort)
            {
                case "totalScore":
                    recentScores = await query.OrderBy(s => s.TotalScore).Take(scoresAmount).ToListAsync(ct);
                    break;
                case "date":
                    recentScores = await query.OrderBy(s => s.Date).Take(scoresAmount).ToListAsync(ct);
                    break;
                default:
                    recentScores = await query.OrderBy(s => s.PP).Take(scoresAmount).ToListAsync(ct);
                    break;
            }
        }
        
        return recentScores;
    } 
}