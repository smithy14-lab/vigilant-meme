namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum DayOfWeekEnum
{
    Monday = 1, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
}

public class Class : SoftDeletableTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? TermId { get; set; }
    public Term? Term { get; set; }
    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public DayOfWeekEnum DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Capacity { get; set; }
    public CheerLevel? Level { get; set; }
    public decimal PricePerSession { get; set; }
    public decimal? TermPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ClassCoach> Coaches { get; set; } = new List<ClassCoach>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<ClassSession> Sessions { get; set; } = new List<ClassSession>();
}

public class ClassCoach : TenantEntity
{
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;
    public Guid CoachId { get; set; }
    public Coach Coach { get; set; } = null!;
    public bool IsLead { get; set; }
}
