using Microsoft.EntityFrameworkCore;
namespace OsuScoreStats.DbService.Repositories;

public interface IRepository<T>
    where T : class
{
    DbSet<T> GetAll();
    Task<T?> GetAsync(int id, CancellationToken ct);
    Task<int> CreateAsync(T item, CancellationToken ct);
    Task<int> CreateBulkAsync(IEnumerable<T> items, CancellationToken ct);
    Task<int> UpdateAsync(T item, CancellationToken ct);
    Task<int> UpdateBulkAsync(IEnumerable<T> items, CancellationToken ct);
    Task<int> DeleteAsync(int id, CancellationToken ct);
    Task<int> DeleteBulkAsync(IEnumerable<int> items, CancellationToken ct);
}
