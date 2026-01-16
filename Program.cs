using Microsoft.EntityFrameworkCore;
using OsuScoreStats;
using OsuScoreStats.DbService;
using OsuScoreStats.ApiClasses;
using OsuScoreStats.Calculators;
using OsuScoreStats.ScoreFetcherService;
using System.Threading.RateLimiting;
using Newtonsoft.Json;
using OsuScoreStats.ApiMethods;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddDbContextFactory<ScoreDataContext>(opt =>
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
        TokenLimit = 59,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 120,
        ReplenishmentPeriod = TimeSpan.FromSeconds(1),
        TokensPerPeriod = 1,
        AutoReplenishment = true
    })
);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<OsuApiService>();
builder.Services.AddSingleton<ICalculator, ScoreCalculator>();
builder.Services.AddSingleton<IScoreFetcher, ScoreFetcher>();
//builder.Services.AddHostedService<ScoreWorker>();
builder.Services.AddScoped<ScoreMethods>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/recentscores", async (ScoreMethods scoreMethods) =>
    {
        var recentScores = await scoreMethods.GetRecentScoresAsync();
        return JsonConvert.SerializeObject(recentScores);
    })
    .WithName("GetRecentScores")
    .WithOpenApi();

app.Run();