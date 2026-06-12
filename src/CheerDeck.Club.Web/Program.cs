using CheerDeck.Infrastructure;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Identity;
using CheerDeck.Infrastructure.Hubs;
using CheerDeck.Club.Web.Components;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddCheerDeckInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
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

    var result = await signInManager.PasswordSignInAsync(email, password, rememberMe, lockoutOnFailure: false);
    if (result.Succeeded)
        return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);

    return Results.Redirect($"/account/login?error=InvalidCredentials&returnUrl={Uri.EscapeDataString(returnUrl ?? "")}");
}).DisableAntiforgery();

app.MapPost("/account/perform-register", async (
    HttpContext context,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager) =>
{
    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();
    var fullName = form["fullName"].ToString();

    var user = new AppUser
    {
        UserName = email,
        Email = email,
        FullName = fullName,
        TenantId = SeedData.ClubTenantId
    };

    var result = await userManager.CreateAsync(user, password);
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(user, AppRoles.Guardian);
        await signInManager.SignInAsync(user, isPersistent: false);
        return Results.Redirect("/");
    }

    var errors = string.Join(",", result.Errors.Select(e => e.Code));
    return Results.Redirect($"/account/register?error={errors}");
}).DisableAntiforgery();

app.MapPost("/account/perform-logout", async (SignInManager<AppUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/account/login");
}).DisableAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<ChatHub>("/hubs/chat");

app.Run();
