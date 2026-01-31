using osu.Game.Rulesets.Scoring;
namespace OsuScoreStats.OsuApi.OsuApiClasses;

public class HitResultAttribute : Attribute
{
    public HitResult HitResult { get; }
        
    public HitResultAttribute(HitResult hitResult)
    {
        HitResult = hitResult;
    }
}