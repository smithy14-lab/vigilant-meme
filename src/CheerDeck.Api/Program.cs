using CheerDeck.Infrastructure;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCheerDeckInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApps", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

app.UseCors("AllowApps");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RunningOrderHub>("/hubs/running-order");
app.MapHub<ScoreHub>("/hubs/scores");
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

app.Run();

public partial class Program { }
