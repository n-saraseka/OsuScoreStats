using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class BeatmapMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get a beatmap from the API
    /// </summary>
    /// <param name="beatmapId">Beatmap ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated APIBeatmap object (or null)</returns>
    public async Task<APIBeatmap?> GetBeatmapAsync(int beatmapId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var beatmapRepository = new BeatmapRepository(dbContext);
        
        return await beatmapRepository.GetAsync(beatmapId, ct);
    }
    
    /// <summary>
    /// Get beatmaps from the API
    /// </summary>
    /// <param name="beatmapIds">Array containing beatmap IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated APIBeatmap objects</returns>
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

    /// <summary>
    /// Get beatmapsets from the API
    /// </summary>
    /// <param name="beatmapsetIds">Array containing beatmapset IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated APIBeatmap objects</returns>
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