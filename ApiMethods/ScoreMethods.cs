using Microsoft.EntityFrameworkCore;
using OsuScoreStats.ApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class ScoreMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get recently fetched scores
    /// </summary>
    /// <param name="mode">Gameplay mode (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="country">Country code</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing 100 most recent scores</returns>
    public async Task<IEnumerable<Score>> GetRecentScoresAsync(
        Mode? mode, 
        string? country,
        string[]? mandatoryMods,
        string[]? optionalMods,
        CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var scoreRepository = new ScoreRepository(dbContext);
        
        var query = scoreRepository.GetAll().AsQueryable();
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
        
        query = query.OrderByDescending(o => o.Date)
                    .ThenByDescending(o => o.Id);
        
        var recentScores = await query.Take(100).ToListAsync(ct);
        return recentScores;
    }
    
    /// <summary>
    /// Get highest PP scores
    /// </summary>
    /// <param name="mode">Gameplay mode (Osu, Taiko, Fruits, Mania)</param>
    /// <param name="date">Date to get scores from (defaults to latest date in scores table)</param>
    /// <param name="country">Country code</param>
    /// <param name="mandatoryMods">An array of mandatory mod acronyms</param>
    /// <param name="optionalMods">An array of optional mod acronyms</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns></returns>
    public async Task<IEnumerable<Score>> GetHighestPpScoresAsync(
        Mode? mode, 
        DateOnly? date,
        string? country,
        string[]? mandatoryMods,
        string[]? optionalMods,
        CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var scoreRepository = new ScoreRepository(dbContext);
        
        
        
        var query = scoreRepository.GetAll().AsQueryable();
        
        var latestDate = await query.MaxAsync(s => s.Date, ct);
        var targetDate = date ?? DateOnly.FromDateTime(latestDate);
        query = query.Where(s => DateOnly.FromDateTime(s.Date).Equals(targetDate));
        
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

        var recentScores = await query.OrderByDescending(s => s.PP).Take(100).ToListAsync(ct); 
        return recentScores;
    } 
}