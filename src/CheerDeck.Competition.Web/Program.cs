using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CheerDeck.Infrastructure;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Identity;
using CheerDeck.Infrastructure.Hubs;
using CheerDeck.Competition.Web.Components;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddCheerDeckInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
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

await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/account/perform-login", async (
    HttpContext context,
    SignInManager<AppUser> signInManager) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var rememberMe = form.ContainsKey("rememberMe");
    var returnUrl = form["returnUrl"].ToString();

    var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: true);
    if (result.Succeeded)
    {
        var safeUrl = !string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
            ? returnUrl : "/";
        return Results.Redirect(safeUrl);
    }

    if (result.IsLockedOut)
        return Results.Redirect("/account/login?error=LockedOut");

    return Results.Redirect($"/account/login?error=InvalidCredentials&returnUrl={Uri.EscapeDataString(returnUrl ?? "")}");
}).DisableAntiforgery().RequireRateLimiting("auth");

app.MapPost("/account/perform-register", async (
    HttpContext context,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AppDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();
    var fullName = form["fullName"].ToString();
    var orgName = form["orgName"].ToString();

    if (password != confirmPassword)
        return Results.Redirect("/account/register?error=PasswordMismatch");

    if (string.IsNullOrWhiteSpace(orgName))
        return Results.Redirect("/account/register?error=OrgNameRequired");

    var slug = orgName.ToLower().Replace(" ", "-").Replace("'", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9-]", "");
    var baseSlug = slug;
    var counter = 1;
    while (await db.Tenants.AnyAsync(t => t.Slug == slug))
    {
        slug = $"{baseSlug}-{counter++}";
    }

    var tenant = new CheerDeck.Domain.Common.Tenant
    {
        Name = orgName.Trim(),
        Slug = slug,
        Type = CheerDeck.Domain.Common.TenantType.EventProducer,
        ContactEmail = email,
        IsActive = true
    };
    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();

    var user = new AppUser
    {
        UserName = email,
        Email = email,
        FullName = fullName,
        TenantId = tenant.Id
    };

    var result = await userManager.CreateAsync(user, password);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, AppRoles.EventProducer);
        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.Redirect("/");
    }

    db.Tenants.Remove(tenant);
    await db.SaveChangesAsync();

    var errors = string.Join(",", result.Errors.Select(e => e.Code));
    return Results.Redirect($"/account/register?error={errors}");
}).DisableAntiforgery().RequireRateLimiting("auth");

app.MapPost("/account/perform-logout", async (SignInManager<AppUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/account/login");
}).DisableAntiforgery();

app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<RunningOrderHub>("/hubs/running-order");
app.MapHub<ScoreHub>("/hubs/scores");
app.MapHub<LeaderboardHub>("/hubs/leaderboard");

app.Run();

public partial class Program { }
