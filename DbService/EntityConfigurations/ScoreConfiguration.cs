using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using osu.Game.Online.API;
using OsuScoreStats.ApiClasses;
namespace OsuScoreStats.DbService.EntityConfigurations;

public class ScoreConfiguration : IEntityTypeConfiguration<Score>
{
    public void Configure(EntityTypeBuilder<Score> builder)
    {
        builder.ComplexProperty(s => s.Statistics, stat => stat.ToJson());
        builder.ComplexProperty(s => s.MaximumStatistics, stat => stat.ToJson());
        builder
            .HasOne<APIBeatmap>()
            .WithMany()
            .HasForeignKey(s => s.BeatmapId);
        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId);
    }
}