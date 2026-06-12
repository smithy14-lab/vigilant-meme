namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Common;

public enum EntryStatus
{
    Pending,
    EligibilityChecked,
    PaymentPending,
    Confirmed,
    Withdrawn,
    Rejected
}

public class EventEntry : TenantEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public Guid DivisionId { get; set; }
    public Division Division { get; set; } = null!;
    public Guid ClubTenantId { get; set; }
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public string? TeamName { get; set; }
    public string? ClubName { get; set; }
    public string? ContactEmail { get; set; }
    public int? EstimatedTeamSize { get; set; }
    public string? EntryNumber { get; set; }
    public EntryStatus Status { get; set; } = EntryStatus.Pending;
    public string? EligibilityNotes { get; set; }
    public bool EligibilityPassed { get; set; }
    public decimal EntryFee { get; set; }
    public string? PaymentId { get; set; }
    public DateTime? PaidAt { get; set; }
    public Guid? MusicFileId { get; set; }
    public TeamMusic? MusicFile { get; set; }
    public bool MusicLicenceVerified { get; set; }
    public string? MusicFileName { get; set; }
    public TimeSpan? MusicDuration { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EntryAthlete> Athletes { get; set; } = new List<EntryAthlete>();
}

public class EntryAthlete : TenantEntity
{
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public bool IsAlternate { get; set; }
    public bool EligibilityPassed { get; set; }
    public string? EligibilityNotes { get; set; }
}
