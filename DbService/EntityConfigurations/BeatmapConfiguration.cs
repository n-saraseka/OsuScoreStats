using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OsuScoreStats.OsuApi.OsuApiClasses;
namespace OsuScoreStats.DbService.EntityConfigurations;

public class BeatmapConfiguration : IEntityTypeConfiguration<APIBeatmap>
{
    public void Configure(EntityTypeBuilder<APIBeatmap> builder)
    {
        builder
            .HasOne(b => b.Beatmapset)
            .WithMany()
            .HasForeignKey(b => b.BeatmapsetId);
    }
}