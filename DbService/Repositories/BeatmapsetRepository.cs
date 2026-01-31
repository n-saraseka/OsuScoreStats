using Microsoft.EntityFrameworkCore;
using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.DbService.Repositories;

public class BeatmapsetRepository(ScoreDataContext db) : IRepository<Beatmapset>
    {

        public DbSet<Beatmapset> GetAll()
        {
            return db.Beatmapsets;
        }

        public async Task<Beatmapset?> GetAsync(int id, CancellationToken ct = default)
        {
            return await db.Beatmapsets.FindAsync(new object[] { id }, ct);
        }

        public async Task<int> CreateAsync(Beatmapset beatmapset, CancellationToken ct = default)
        {
            db.Beatmapsets.Add(beatmapset);
            return await db.SaveChangesAsync(ct);
        }

        public async Task<int> CreateBulkAsync(IEnumerable<Beatmapset> beatmapsets, CancellationToken ct = default)
        {
            db.Beatmapsets.AddRange(beatmapsets);
            return await db.SaveChangesAsync(ct);
        }

        public async Task<int> UpdateAsync(Beatmapset beatmapset, CancellationToken ct = default)
        {
            db.Beatmapsets.Update(beatmapset);
            return await db.SaveChangesAsync(ct);
        }

        public async Task<int> UpdateBulkAsync(IEnumerable<Beatmapset> beatmapsets, CancellationToken ct = default)
        {
            db.Beatmapsets.UpdateRange(beatmapsets);
            return await db.SaveChangesAsync(ct);
        }

        public async Task<int> DeleteAsync(int id, CancellationToken ct = default)
        {
            var beatmapset = await GetAsync(id, ct);
            if (beatmapset != null)
                db.Beatmapsets.Remove(beatmapset);
            return await db.SaveChangesAsync(ct);
        }

        public async Task<int> DeleteBulkAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var beatmapsetsToDelete = await db.Beatmapsets.Where(score => ids.Contains((int)score.Id)).ToListAsync(ct);
            db.Beatmapsets.RemoveRange(beatmapsetsToDelete);
            return await db.SaveChangesAsync(ct);
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
