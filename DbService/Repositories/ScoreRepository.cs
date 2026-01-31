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

    public async Task<int> CreateAsync(Score score, CancellationToken ct = default)
    {
        var existingScore = db.Scores.FirstOrDefault(s => s.Id == score.Id);
        if (existingScore == null)
            db.Scores.Add(score);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> CreateBulkAsync(IEnumerable<Score> scores, CancellationToken ct = default)
    {
        var existingScoreIds = await db.Scores
            .Select(score => score.Id)
            .Where(id => scores.Select(score => score.Id).Contains(id))
            .ToListAsync(ct);
        var newScores = scores.Where(score => !existingScoreIds.Contains(score.Id));
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
