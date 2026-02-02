using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.DbService.Repositories;

public class UserRepository(ScoreDataContext db) : IRepository<User>
{

    public DbSet<User> GetAll()
    {
        return db.Users;
    }

    public async Task<User?> GetAsync(int id, CancellationToken ct = default)
    {
        return await db.Users.FindAsync(new object[] { id }, ct);
    }

    public async Task<User?> GetExistingAsync(User user, CancellationToken ct = default)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id, ct);
    }

    public async Task<IEnumerable<User>> GetExistingBulkAsync(IEnumerable<User> users, CancellationToken ct = default)
    {
        var existingUsers = await db.Users
            .Where(user => users.Select(u => u.Id).Contains(user.Id))
            .ToListAsync(ct);
        return existingUsers;
    }

    public async Task<int> CreateAsync(User user, CancellationToken ct = default)
    {
        var countryRepository = new CountryRepository(db);
        await countryRepository.CreateAsync(user.Country, ct);
        
        var existingUser = await GetExistingAsync(user, ct);
        if (existingUser == null)
            db.Users.Add(user);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> CreateBulkAsync(IEnumerable<User> users, CancellationToken ct = default)
    {
        var countryRepository = new CountryRepository(db);
        var userCountries = users
            .GroupBy(u => u.CountryCode)
            .Select(g => g.First().Country)
            .ToList();
        await countryRepository.CreateBulkAsync(userCountries, ct);

        var existingCountries = countryRepository.GetAll();
        foreach (var user in users)
            user.Country = existingCountries.FirstOrDefault(c => c.Code == user.Country.Code);
        
        var existingUsers = await GetExistingBulkAsync(users, ct);
        var newUsers = users.Where(u => !existingUsers.Select(user => user.Id).Contains(u.Id));
        var usersToUpdate = users.Where(u => existingUsers.Select(user => user.Id).Contains(u.Id));

        db.Users.UpdateRange(usersToUpdate);
        db.Users.AddRange(newUsers);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(User user, CancellationToken ct)
    {
        var countryRepository = new CountryRepository(db);
        var existingCountries = countryRepository.GetAll();
        var userCountry = existingCountries.FirstOrDefault(c => c.Code == user.Country.Code);
        if (userCountry != null)
            user.Country = userCountry;
        db.Users.Update(user);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateBulkAsync(IEnumerable<User> users, CancellationToken ct)
    {
        var countryRepository = new CountryRepository(db);
        var existingCountries = countryRepository.GetAll();
        foreach (var user in users)
        {
            var userCountry = existingCountries.FirstOrDefault(c => c.Code == user.Country.Code);
            if (userCountry != null)
                user.Country = userCountry;
        }
        db.Users.UpdateRange(users);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteAsync(int id, CancellationToken ct)
    {
        var user = await GetAsync(id, ct);
        if (user != null)
            db.Users.Remove(user);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteBulkAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var usersToDelete = await db.Users.Where(user => ids.Contains(user.Id)).ToListAsync(ct);
        db.Users.RemoveRange(usersToDelete);
        return await db.SaveChangesAsync(ct);
    }
}
