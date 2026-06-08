namespace CheerDeck.Infrastructure.Data;

using CheerDeck.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

public class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public Guid TenantId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }

    public string? UserId => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    public string? UserRole => httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
}

public class FixedTenantContext(Guid tenantId, string? userId = null, string? role = null) : ITenantContext
{
    public Guid TenantId => tenantId;
    public string? UserId => userId;
    public string? UserRole => role;
}
