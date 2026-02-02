using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
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
    
    public async Task<APIBeatmap?> GetExistingAsync(APIBeatmap beatmap, CancellationToken ct = default)
    {
        return await db.Beatmaps.FirstOrDefaultAsync(b => b.Id == beatmap.Id, ct);
    }

    public async Task<IEnumerable<APIBeatmap>> GetExistingBulkAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct = default)
    {
        var existingBeatmaps = await db.Beatmaps
            .Where(beatmap => beatmaps.Select(b => b.Id).Contains(beatmap.Id))
            .ToListAsync(ct);
        return existingBeatmaps;
    }

    public async Task<int> CreateAsync(APIBeatmap beatmap, CancellationToken ct = default)
    {
        if (beatmap.Beatmapset != null)
        {
            var beatmapsetRepository = new BeatmapsetRepository(db);
            await beatmapsetRepository.CreateAsync(beatmap.Beatmapset, ct);
        }
        
        var existingBeatmap = db.Beatmaps.FirstOrDefault(b => b.Id == beatmap.Id);
        if (existingBeatmap == null)
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
        await beatmapsetRepository.CreateBulkAsync(beatmapsets, ct);

        var existingBeatmapsets = beatmapsetRepository.GetAll();
        foreach (var beatmap in beatmaps)
            beatmap.Beatmapset = existingBeatmapsets.FirstOrDefault(bs => bs.Id == beatmap.Beatmapset.Id);

        db.Beatmaps.AddRange(beatmaps);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(APIBeatmap beatmap, CancellationToken ct)
    {
        var beatmapsetRepository = new BeatmapsetRepository(db);
        var existingBeatmapsets = beatmapsetRepository.GetAll();
        var beatmapset = existingBeatmapsets.FirstOrDefault(bs => bs.Id == beatmap.Beatmapset.Id);
        if (beatmapset != null)
            beatmap.Beatmapset = beatmapset;
        db.Beatmaps.Update(beatmap);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateBulkAsync(IEnumerable<APIBeatmap> beatmaps, CancellationToken ct)
    {
        var beatmapsetRepository = new BeatmapsetRepository(db);
        var existingBeatmapsets = beatmapsetRepository.GetAll();
        foreach (var beatmap in beatmaps)
        {
            var beatmapset = existingBeatmapsets.FirstOrDefault(bs => bs.Id == beatmap.Beatmapset.Id);
            if (beatmapset != null)
                beatmap.Beatmapset = beatmapset;
        }
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
