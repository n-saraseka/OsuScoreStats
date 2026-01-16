using Microsoft.EntityFrameworkCore;
using OsuScoreStats.ApiClasses;
namespace OsuScoreStats.DbService.Repositories;

public class BeatmapRepository(ScoreDataContext db) : IRepository<APIBeatmap>
    {

       public DbSet<APIBeatmap> GetAll()
    {
        return db.Beatmaps;
    }

    public async Task<APIBeatmap?> GetAsync(int id, CancellationToken ct = default)
    {
        return await db.Beatmaps.FindAsync(new object[] { id }, ct);
    }

    public async Task<int> CreateAsync(APIBeatmap beatmap, CancellationToken ct = default)
    {
        var beatmapsetRepository = new BeatmapsetRepository(db);
        var beatmapset = await beatmapsetRepository.GetAsync(beatmap.BeatmapsetId, ct);
        if (beatmapset == null)
            await beatmapsetRepository.CreateAsync(beatmap.Beatmapset, ct);
        db.Beatmaps.Add(beatmap);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> CreateBulkAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct = default)
    {
        var beatmapsetRepository = new BeatmapsetRepository(db);
        var beatmapsets = beatmaps
            .GroupBy(b => b.BeatmapsetId)
            .Select(g => g.First().Beatmapset)
            .ToList();
        var existingBeatmapsetIds = beatmapsetRepository.GetAll().Select(bs => bs.Id).ToList();
        var newBeatmapsets = beatmapsets.Where(bs => !existingBeatmapsetIds.Contains(bs.Id)).ToList();
        if (newBeatmapsets.Count > 0)
            await beatmapsetRepository.CreateBulkAsync(newBeatmapsets, ct);

        var existingBeatmapsets = beatmapsetRepository.GetAll();
        foreach (var beatmap in beatmaps)
            beatmap.Beatmapset = existingBeatmapsets.FirstOrDefault(bs => bs.Id == beatmap.Beatmapset.Id);

        db.Beatmaps.AddRange(beatmaps);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(APIBeatmap beatmap, CancellationToken ct)
    {
        db.Beatmaps.Update(beatmap);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateBulkAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct)
    {
        db.Beatmaps.UpdateRange(beatmaps);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteAsync(int id, CancellationToken ct)
    {
        var beatmap = await GetAsync(id, ct);
        if (beatmap != null)
            db.Beatmaps.Remove(beatmap);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteBulkAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var beatmapsToDelete = await db.Beatmaps.Where(b => ids.Contains(b.Id)).ToListAsync(ct);
        db.Beatmaps.RemoveRange(beatmapsToDelete);
        return await db.SaveChangesAsync(ct);
    }
}
