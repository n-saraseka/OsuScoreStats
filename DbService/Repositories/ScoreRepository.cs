using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.DbService.Repositories;

public class ScoreRepository(ScoreDataContext db) : IRepository<Score>
{

    public DbSet<Score> GetAll()
    {
        return db.Scores;
    }

    public async Task<Score?> GetAsync(int id, CancellationToken ct = default)
    {
        return await db.Scores.FindAsync(new object[] { id }, ct);
    }
    
    public async Task<Score?> GetExistingAsync(Score score, CancellationToken ct = default)
    {
        return await db.Scores.FirstOrDefaultAsync(s => s.Id == score.Id, ct);
    }

    public async Task<IEnumerable<Score>> GetExistingBulkAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var existingScores = await db.Scores
            .Where(score => scores.Select(s => s.Id).Contains(score.Id))
            .ToListAsync(ct);
        return existingScores;
    }

    public async Task<int> CreateAsync(Score score, CancellationToken ct = default)
    {
        var existingScore = await GetExistingAsync(score, ct);
        if (existingScore == null)
            db.Scores.Add(score);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> CreateBulkAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var existingScoreIds = await GetExistingBulkAsync(scores, ct);
        var newScores = scores.Where(score => !existingScoreIds.Select(s => s.Id).Contains(score.Id));
        db.Scores.AddRange(newScores);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(Score score, CancellationToken ct = default)
    {
        db.Scores.Update(score);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateBulkAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        db.Scores.UpdateRange(scores);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteAsync(int id, CancellationToken ct = default)
    {
        var score = await GetAsync(id, ct);
        if (score != null)
            db.Scores.Remove(score);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteBulkAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var scoresToDelete = await db.Scores.Where(score => ids.Contains((int)score.Id)).ToListAsync(ct);
        db.Scores.RemoveRange(scoresToDelete);
        return await db.SaveChangesAsync(ct);
    }
}
