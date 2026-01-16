using Microsoft.EntityFrameworkCore;
using OsuScoreStats.ApiClasses;
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

    public async Task<int> CreateAsync(User user, CancellationToken ct = default)
    {
        var countryRepository = new CountryRepository(db);
        var userCountry = await countryRepository.GetAsync(user.CountryCode, ct);
        if (userCountry == null)
            await countryRepository.CreateAsync(user.Country, ct);
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
        var existingCountryCodes = countryRepository.GetAll().Select(c => c.Code).ToList();
        var newCountries = userCountries.Where(c => !existingCountryCodes.Contains(c.Code)).ToList();
        if (newCountries.Count > 0)
            await countryRepository.CreateBulkAsync(newCountries, ct);

        var existingCountries = countryRepository.GetAll();
        foreach (var user in users)
            user.Country = existingCountries.FirstOrDefault(c => c.Code == user.Country.Code);

        db.Users.AddRange(users);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(User user, CancellationToken ct)
    {
        db.Users.Update(user);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateBulkAsync(IEnumerable<User> users, CancellationToken ct)
    {
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
