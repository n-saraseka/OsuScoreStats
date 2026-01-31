using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.ScoreFetcherService;

public interface IScoreFetcher
{
    public Task<ScoresResponse> GetScoresAsync(string? cursor, CancellationToken ct);
    public Task ProcessUnrankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct);
    public Task ProcessRankedScoresAsync(IEnumerable<Score> scores, CancellationToken ct);
    public Task ProcessUsersAsync(IEnumerable<int> userIds, CancellationToken ct);
    public Task ProcessBeatmapsAsync(IEnumerable<int> beatmapIds, CancellationToken ct);
}