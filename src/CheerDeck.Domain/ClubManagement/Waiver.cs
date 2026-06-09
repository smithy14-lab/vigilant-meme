namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum WaiverType
{
    LiabilityWaiver,
    MediaConsent,
    MedicalConsent,
    CodeOfConduct,
    TermsAndConditions,
    Custom
}

public enum WaiverStatus
{
    Active,
    Archived,
    Draft
}

public class Waiver : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public WaiverType Type { get; set; }
    public WaiverStatus Status { get; set; } = WaiverStatus.Active;
    public int Version { get; set; } = 1;
    public bool RequiredForEnrolment { get; set; }
    public DateOnly? ExpiresOn { get; set; }

    public ICollection<WaiverSignature> Signatures { get; set; } = new List<WaiverSignature>();
}

public class WaiverSignature : TenantEntity
{
    public Guid WaiverId { get; set; }
    public Waiver Waiver { get; set; } = null!;
    public Guid GuardianId { get; set; }
    public Guardian Guardian { get; set; } = null!;
    public Guid? AthleteId { get; set; }
    public Athlete? Athlete { get; set; }
    public string SignedByName { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
}
