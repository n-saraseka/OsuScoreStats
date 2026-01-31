using Microsoft.EntityFrameworkCore;
using OsuScoreStats.DbService;
using OsuScoreStats.OsuApi.OsuApiClasses;
using OsuScoreStats.Calculators;
using OsuScoreStats.ScoreFetcherService;
using System.Threading.RateLimiting;
using OsuScoreStats.ApiMethods;
using OsuScoreStats.OsuApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddPooledDbContextFactory<ScoreDataContext>(
    opt =>
    opt.UseNpgsql(
            builder.Configuration["DefaultConnection"],
            o => o
                .MapEnum<Mode>("mode")
                .MapEnum<Grade>("grade")
                .MapEnum<BeatmapStatus>("beatmap_status"))
                .UseSnakeCaseNamingConvention());

builder.Services.AddHttpClient();
builder.Services.AddSingleton<RateLimiter>(sp => 
    new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
    {
        TokenLimit = 1,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 60,
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        TokensPerPeriod = 1,
        AutoReplenishment = true
    })
);

builder.Services.AddSingleton<OsuApiService>();
builder.Services.AddSingleton<ICalculator, ScoreCalculator>();
builder.Services.AddSingleton<IScoreFetcher, ScoreFetcher>();
//builder.Services.AddHostedService<ScoreWorker>();
builder.Services.AddScoped<ScoreMethods>();
builder.Services.AddScoped<BeatmapMethods>();
builder.Services.AddScoped<UserMethods>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

var contextFactory = app.Services.GetRequiredService<IDbContextFactory<ScoreDataContext>>();
using var context = contextFactory.CreateDbContext();
context.Database.EnsureCreated();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();

app.MapGet("/scores", async (
        ScoreMethods scoreMethods, 
        Mode? mode, 
        DateOnly? date,
        string? country,
        string[]? mandatoryMods,
        string[]? optionalMods,
        int? amount,
        string? sort,
        bool isDesc,
        CancellationToken ct) =>
    {
        return await scoreMethods.GetScoresAsync(mode, date, country, mandatoryMods, optionalMods, amount, sort, isDesc, ct);
    })
    .WithName("GetScores")
    .WithOpenApi();

app.MapGet("/beatmap", async (
        BeatmapMethods beatmapMethods, 
        int beatmapId,
        CancellationToken ct) =>
    {
        return await beatmapMethods.GetBeatmapAsync(beatmapId, ct);
    })
    .WithName("GetBeatmap")
    .WithOpenApi();

app.MapGet("/beatmaps", async (
        BeatmapMethods beatmapMethods, 
        int[] beatmapIds,
        CancellationToken ct) =>
    {
        return await beatmapMethods.GetBeatmapsAsync(beatmapIds, ct);
    })
    .WithName("GetBeatmaps")
    .WithOpenApi();

app.MapGet("/beatmapsets", async (
        BeatmapMethods beatmapMethods, 
        int[] beatmapsetIds,
        CancellationToken ct) =>
    {
        return await beatmapMethods.GetBeatmapsetsAsync(beatmapsetIds, ct);
    })
    .WithName("GetBeatmapsets")
    .WithOpenApi();

app.MapGet("/user", async (
        UserMethods userMethods, 
        int userId,
        CancellationToken ct) =>
    {
        return await userMethods.GetUserAsync(userId, ct);
    })
    .WithName("GetUser")
    .WithOpenApi();

app.MapGet("/users", async (
        UserMethods userMethods, 
        int[] userIds,
        CancellationToken ct) =>
    {
        return await userMethods.GetUsersAsync(userIds, ct);
    })
    .WithName("GetUsers")
    .WithOpenApi();

app.MapControllers();
app.Run();