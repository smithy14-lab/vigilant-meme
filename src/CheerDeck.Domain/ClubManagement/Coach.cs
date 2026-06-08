namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public class Coach : SoftDeletableTenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? UserId { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<CoachQualification> Qualifications { get; set; } = new List<CoachQualification>();
    public ICollection<ClassCoach> ClassAssignments { get; set; } = new List<ClassCoach>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}

public class CoachQualification : TenantEntity
{
    public Guid CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public string QualificationType { get; set; } = string.Empty;
    public string? CertificateNumber { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public bool IsVerified { get; set; }
    public string? ExternalCredentialId { get; set; }

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
}
