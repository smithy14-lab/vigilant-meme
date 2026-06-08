using CheerDeck.Infrastructure;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Hubs;
using CheerDeck.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCheerDeckInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<RunningOrderHub>("/hubs/running-order");
app.MapHub<ScoreHub>("/hubs/scores");
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

app.Run();

public partial class Program { }
