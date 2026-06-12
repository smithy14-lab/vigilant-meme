using Microsoft.AspNetCore.RateLimiting;
using CheerDeck.Api.Middleware;
using CheerDeck.Infrastructure;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<GlobalExceptionHandler>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CheerDeck API", Version = "v1" });
});
builder.Services.AddCheerDeckInfrastructure(builder.Configuration);
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApps", policy =>
    {
        if (allowedOrigins is { Length: > 0 })
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        else
            policy.WithOrigins("https://localhost:5200", "https://localhost:5300").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("auth", cfg =>
    {
        cfg.PermitLimit = 10;
        cfg.Window = TimeSpan.FromMinutes(1);
        cfg.QueueLimit = 0;
    });
});

var app = builder.Build();

try
{
    await SeedData.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Database initialization failed — app will start without seeded data");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseMiddleware<GlobalExceptionHandler>();
app.UseCors("AllowApps");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapGet("/health/startup", async (IServiceProvider sp) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("DefaultConnection");
    var useInMemory = string.IsNullOrEmpty(connStr) || config.GetValue<bool>("UseInMemoryDatabase");
    var dbStatus = "unknown";

    try
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CheerDeck.Infrastructure.Data.AppDbContext>();
        var canConnect = await db.Database.CanConnectAsync();
        dbStatus = canConnect ? "connected" : "cannot-connect";
    }
    catch (Exception ex)
    {
        dbStatus = $"error: {ex.GetType().Name}: {ex.Message}";
    }

    return Results.Ok(new
    {
        status = "running",
        connectionStringPresent = !string.IsNullOrEmpty(connStr),
        useInMemory,
        dbStatus,
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown"
    });
});
app.MapControllers();
app.MapHub<RunningOrderHub>("/hubs/running-order");
app.MapHub<ScoreHub>("/hubs/scores");
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

app.Run();

public partial class Program { }
