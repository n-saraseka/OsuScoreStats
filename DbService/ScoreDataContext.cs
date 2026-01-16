using Microsoft.EntityFrameworkCore;
using OsuScoreStats.ApiClasses;
using OsuScoreStats.DbService.EntityConfigurations;
namespace OsuScoreStats.DbService;

public class ScoreDataContext(DbContextOptions<ScoreDataContext> options) : DbContext(options)
{
    public DbSet<APIBeatmap> Beatmaps { get; set; }
    public DbSet<Beatmapset> Beatmapsets { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ScoreConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new BeatmapConfiguration());
        modelBuilder.ApplyConfiguration(new CountryConfiguration());
    }
}
    