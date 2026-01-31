using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.DbService;
using OsuScoreStats.DbService.Repositories;
namespace OsuScoreStats.ApiMethods;

public class UserMethods(IDbContextFactory<ScoreDataContext> dbContextFactory)
{
    public async Task<User?> GetUserAsync(int userId, CancellationToken ct = default)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync(ct);
        var userRepository = new UserRepository(dbContext);
        
        return await userRepository.GetAsync(userId, ct);
    }
    
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