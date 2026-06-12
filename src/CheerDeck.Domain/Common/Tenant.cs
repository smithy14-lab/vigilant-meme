namespace CheerDeck.Domain.Common;

public enum TenantType
{
    Club,
    EventProducer
}

public enum SubscriptionPlan
{
    Free,
    Starter,
    Growth,
    Professional
}

public enum SubscriptionStatus
{
    None,
    Active,
    PastDue,
    Cancelled,
    Trialing
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

    public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.None;
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime? SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
    public bool IsLinkedToCompetitions { get; set; }
}
