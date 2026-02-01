using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class UserMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    /// <summary>
    /// Get user data from the API
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Populated User object (or null)</returns>
    public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        return await userRepository.GetAsync(userId, ct);
    }
    
    /// <summary>
    /// Get users data from the API
    /// </summary>
    /// <param name="userIds">Array containing user IDs</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>IEnumerable containing populated User objects</returns>
    public async Task<IEnumerable<User>> GetUsersAsync(int[] userIds, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        var users = await userRepository
            .GetAll()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);
        
        return users;
    }
}