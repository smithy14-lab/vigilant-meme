namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum EnrolmentStatus
{
    Active,
    Cancelled,
    WaitingList,
    Completed
}

public class Enrolment : TenantEntity
{
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;
    public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Active;
    public DateOnly EnrolledDate { get; set; }
    public DateOnly? CancelledDate { get; set; }
    public int? WaitingListPosition { get; set; }
}
