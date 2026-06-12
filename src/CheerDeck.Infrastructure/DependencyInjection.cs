namespace CheerDeck.Infrastructure;

using CheerDeck.Application.Interfaces;
using CheerDeck.Application.Services;
using CheerDeck.Domain.Integration;
using CheerDeck.Infrastructure.Data;
using CheerDeck.Infrastructure.Identity;
using CheerDeck.Infrastructure.Services;
using CheerDeck.Infrastructure.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCheerDeckInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = string.IsNullOrEmpty(connectionString) ||
                          configuration.GetValue<bool>("UseInMemoryDatabase");

        if (useInMemory)
        {
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseInMemoryDatabase("CheerDeckDb");
            });
        }
        else
        {
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString, sql =>
                    sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            });
        }

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders()
        .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.LogoutPath = "/account/logout";
            options.AccessDeniedPath = "/account/access-denied";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });

        services.AddScoped<HttpTenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<HttpTenantContext>());

        services.AddScoped<IEligibilityProvider, StubEligibilityProvider>();
        services.AddScoped<IMusicLicenceProvider, StubMusicLicenceProvider>();

        if (!string.IsNullOrEmpty(configuration["Stripe:SecretKey"]))
            services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        else
            services.AddScoped<IPaymentGateway, StubPaymentGateway>();

        if (!string.IsNullOrEmpty(configuration["SendGrid:ApiKey"]))
            services.AddScoped<IEmailService, SendGridEmailService>();
        else
            services.AddScoped<IEmailService, StubEmailService>();

        // Application services
        services.AddScoped<AthleteService>();
        services.AddScoped<CoachService>();
        services.AddScoped<ClassService>();
        services.AddScoped<VenueService>();
        services.AddScoped<TermService>();
        services.AddScoped<CampService>();
        services.AddScoped<GuardianService>();
        services.AddScoped<PrivateLessonService>();
        services.AddScoped<InvoiceService>();
        services.AddScoped<MessageService>();
        services.AddScoped<TeamService>();
        services.AddScoped<EventService>();
        services.AddScoped<EntryService>();
        services.AddScoped<RunningOrderService>();
        services.AddScoped<ScoringService>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<WaiverService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<ChatService>();
        services.AddScoped<ReminderService>();

        services.AddSignalR();

        return services;
    }
}
