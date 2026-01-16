using Microsoft.EntityFrameworkCore;
using OsuScoreStats.ApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class ScoreMethods(ScoreDataContext db)
{
    public async Task<IEnumerable<Score>> GetRecentScoresAsync(CancellationToken ct = default)
    {
        var scoreRepository = new ScoreRepository(db);
        var recentScores = await scoreRepository.GetAll().Take(100).ToListAsync(ct);
        
        return recentScores;
    }
}