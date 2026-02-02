using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.DbService.Repositories;

public class CountryRepository(ScoreDataContext db)
{

    public DbSet<Country> GetAll()
    {
        return db.Countries;
    }

    public async Task<Country?> GetAsync(string code, CancellationToken ct = default)
    {
        return await db.Countries.FindAsync(new object[] { code }, ct);
    }
    
    public async Task<Country?> GetExistingAsync(Country country, CancellationToken ct = default)
    {
        return await db.Countries.FirstOrDefaultAsync(c => c.Code == country.Code, ct);
    }

    public async Task<IEnumerable<Country>> GetExistingBulkAsync(IEnumerable<Country> countries, CancellationToken ct = default)
    {
        var existingCountries = await db.Countries
            .Where(country => countries.Select(c => c.Code).Contains(country.Code))
            .ToListAsync(ct);
        return existingCountries;
    }

    public async Task<int> CreateAsync(Country country, CancellationToken ct = default)
    {
        var existingCountry = await GetExistingAsync(country, ct);
        if (existingCountry == null) 
            db.Countries.Add(country);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> CreateBulkAsync(IEnumerable<Country> countries, CancellationToken ct = default)
    {
        var existingCountries = await GetExistingBulkAsync(countries, ct);
        var newCountries = countries
            .Where(country => !existingCountries.Select(c => c.Code).Contains(country.Code));
        db.Countries.AddRange(newCountries);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateAsync(Country country, CancellationToken ct = default)
    {
        db.Countries.Update(country);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> UpdateBulkAsync(IEnumerable<Country> countries, CancellationToken ct = default)
    {
        db.Countries.UpdateRange(countries);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteAsync(string code, CancellationToken ct = default)
    {
        var country = await GetAsync(code, ct);
        if (country != null)
            db.Countries.Remove(country);
        return await db.SaveChangesAsync(ct);
    }

    public async Task<int> DeleteBulkAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        var countriesToDelete = await db.Countries.Where(country => codes.Contains(country.Code)).ToListAsync(ct);
        db.Countries.RemoveRange(countriesToDelete);
        return await db.SaveChangesAsync(ct);
    }
}
