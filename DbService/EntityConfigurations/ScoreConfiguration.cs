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
        builder.Property(s => s.Statistics).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Statistics>(v,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        builder.Property(s => s.MaximumStatistics).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Statistics>(v,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
        builder.Property(s => s.Mods).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<APIMod[]>(v,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
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