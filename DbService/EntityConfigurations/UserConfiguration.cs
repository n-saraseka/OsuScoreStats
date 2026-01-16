using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using OsuScoreStats.ApiClasses;
namespace OsuScoreStats.DbService.EntityConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasOne(u => u.Country)
            .WithMany()
            .HasForeignKey(u => u.CountryCode);
        builder.Property(s => s.RulesetStatistics).HasConversion(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<Dictionary<string, UserRulesetStatistics>>(v,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
    }
}