namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum PrivateLessonStatus
{
    Scheduled,
    Completed,
    Cancelled,
    NoShow
}

public class PrivateLesson : TenantEntity
{
    public Guid CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public PrivateLessonStatus Status { get; set; } = PrivateLessonStatus.Scheduled;
    public string? Notes { get; set; }

    public ICollection<PrivateLessonAthlete> Athletes { get; set; } = new List<PrivateLessonAthlete>();
}

public class PrivateLessonAthlete : TenantEntity
{
    public Guid PrivateLessonId { get; set; }
    public PrivateLesson PrivateLesson { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
}
