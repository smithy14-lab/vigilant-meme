namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public class Team : SoftDeletableTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public CheerLevel Level { get; set; }
    public string? AgeGridDivision { get; set; }
    public Guid? HeadCoachId { get; set; }
    public Coach? HeadCoach { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
    public ICollection<TeamMusic> Music { get; set; } = new List<TeamMusic>();
}

public class TeamMember : TenantEntity
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public string? Position { get; set; }
    public bool IsAlternate { get; set; }
}

public class TeamMusic : TenantEntity
{
    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? LicenceProof { get; set; }
    public bool LicenceVerified { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
