namespace CheerDeck.Domain.Common;

public enum TenantType
{
    Club,
    EventProducer
}

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public TenantType Type { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
