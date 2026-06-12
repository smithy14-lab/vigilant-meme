namespace CheerDeck.Infrastructure.Identity;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

public class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<AppUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (user.TenantId.HasValue)
            identity.AddClaim(new Claim("TenantId", user.TenantId.Value.ToString()));
        if (!string.IsNullOrEmpty(user.FullName))
            identity.AddClaim(new Claim("FullName", user.FullName));
        return identity;
    }
}
