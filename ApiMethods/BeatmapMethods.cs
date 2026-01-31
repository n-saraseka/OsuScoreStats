using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class BeatmapMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    public async Task<APIBeatmap?> GetBeatmapAsync(int beatmapId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        return await beatmapRepository.GetAsync(beatmapId, ct);
    }

    public async Task<IEnumerable<APIBeatmap>> GetBeatmapsAsync(int[] beatmapIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        var beatmaps = await beatmapRepository
            .GetAll()
            .Where(b => beatmapIds.Contains(b.Id))
            .ToListAsync(ct);
        return beatmaps;
    }

    public async Task<IEnumerable<Beatmapset>> GetBeatmapsetsAsync(int[] beatmapsetIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapsetRepository = new BeatmapsetRepository(dbContext);

        var beatmapsets = await beatmapsetRepository
            .GetAll()
            .Where(bs => beatmapsetIds.Contains(bs.Id))
            .ToListAsync(ct);
        return beatmapsets;
    }
}