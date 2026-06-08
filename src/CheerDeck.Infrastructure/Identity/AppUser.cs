namespace CheerDeck.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;

public class AppUser : IdentityUser
{
    public Guid? TenantId { get; set; }
    public string? FullName { get; set; }
}
