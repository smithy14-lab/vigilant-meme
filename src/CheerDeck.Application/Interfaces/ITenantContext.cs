namespace CheerDeck.Application.Interfaces;

public interface ITenantContext
{
    Guid TenantId { get; }
    string? UserId { get; }
    string? UserRole { get; }
}
