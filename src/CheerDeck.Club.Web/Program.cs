using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using CheerDeck.Infrastructure;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Identity;
using CheerDeck.Infrastructure.Hubs;
using CheerDeck.Club.Web.Components;
using Microsoft.AspNetCore.Identity;
using CheerDeck.Application.Interfaces;

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

try
{
    await SeedData.InitializeAsync(app.Services);
}
catch (Exception ex)
{
    CheerDeck.Infrastructure.Data.SeedData.LastInitError = ex.ToString();
    app.Logger.LogCritical(ex, "Database initialization failed — app will start without seeded data");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

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
    var clubName = form["clubName"].ToString();

    if (password != confirmPassword)
        return Results.Redirect("/account/register?error=PasswordMismatch");

    if (string.IsNullOrWhiteSpace(clubName))
        return Results.Redirect("/account/register?error=ClubNameRequired");

    var slug = clubName.ToLower().Replace(" ", "-").Replace("'", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, "[^a-z0-9-]", "");
    var baseSlug = slug;
    var counter = 1;
    while (await db.Tenants.AnyAsync(t => t.Slug == slug))
    {
        slug = $"{baseSlug}-{counter++}";
    }

    var tenant = new CheerDeck.Domain.Common.Tenant
    {
        Name = clubName.Trim(),
        Slug = slug,
        Type = CheerDeck.Domain.Common.TenantType.Club,
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
        await userManager.AddToRoleAsync(user, AppRoles.ClubOwner);
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

app.MapPost("/account/perform-forgot-password", async (HttpRequest request, UserManager<AppUser> userManager, IEmailService emailService) =>
{
    var form = await request.ReadFormAsync();
    var email = form["email"].ToString().Trim();

    if (!string.IsNullOrEmpty(email))
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Net.WebUtility.UrlEncode(token);
            var encodedEmail = System.Net.WebUtility.UrlEncode(email);
            var resetUrl = $"{request.Scheme}://{request.Host}/account/reset-password?email={encodedEmail}&token={encodedToken}";

            await emailService.SendAsync(
                email,
                "Reset your CheerDeck Club password",
                $"""
                <h2>Password Reset</h2>
                <p>You requested a password reset for your CheerDeck Club account.</p>
                <p><a href="{resetUrl}" style="display:inline-block;padding:12px 24px;background:#0d6efd;color:#fff;text-decoration:none;border-radius:6px;">Reset Password</a></p>
                <p>If you didn't request this, you can safely ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
                """);
        }
    }

    return Results.Redirect("/account/forgot-password?Status=Sent");
}).DisableAntiforgery().RequireRateLimiting("auth");

app.MapPost("/account/perform-reset-password", async (HttpRequest request, UserManager<AppUser> userManager) =>
{
    var form = await request.ReadFormAsync();
    var email = form["email"].ToString().Trim();
    var token = form["token"].ToString();
    var password = form["password"].ToString();
    var confirmPassword = form["confirmPassword"].ToString();

    if (password != confirmPassword)
        return Results.Redirect($"/account/reset-password?email={System.Net.WebUtility.UrlEncode(email)}&token={System.Net.WebUtility.UrlEncode(token)}&Status=PasswordMismatch");

    var user = await userManager.FindByEmailAsync(email);
    if (user is null)
        return Results.Redirect("/account/reset-password?Status=InvalidToken");

    var result = await userManager.ResetPasswordAsync(user, token, password);
    if (result.Succeeded)
        return Results.Redirect("/account/reset-password?Status=Success");

    return Results.Redirect($"/account/reset-password?email={System.Net.WebUtility.UrlEncode(email)}&token={System.Net.WebUtility.UrlEncode(token)}&Status=InvalidToken");
}).DisableAntiforgery().RequireRateLimiting("auth");

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
        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "unknown",
        seedError = CheerDeck.Infrastructure.Data.SeedData.LastInitError
    });
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
