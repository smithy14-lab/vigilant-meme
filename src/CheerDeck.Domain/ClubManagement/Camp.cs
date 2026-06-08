namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum CampStatus
{
    Draft,
    Published,
    Full,
    Completed,
    Cancelled
}

public class Camp : SoftDeletableTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public CampStatus Status { get; set; } = CampStatus.Draft;
    public CheerLevel? MinLevel { get; set; }
    public CheerLevel? MaxLevel { get; set; }

    public ICollection<CampBooking> Bookings { get; set; } = new List<CampBooking>();

    public int BookedCount => Bookings.Count(b => b.Status == CampBookingStatus.Confirmed);
    public int WaitingCount => Bookings.Count(b => b.Status == CampBookingStatus.WaitingList);
    public bool IsFull => BookedCount >= Capacity;
}

public enum CampBookingStatus
{
    Confirmed,
    WaitingList,
    Cancelled,
    Invited
}

public class CampBooking : TenantEntity
{
    public Guid CampId { get; set; }
    public Camp Camp { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public CampBookingStatus Status { get; set; }
    public int? WaitingListPosition { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? InviteExpiresAt { get; set; }
}
