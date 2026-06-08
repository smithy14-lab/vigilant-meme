namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.Common;

public enum EventStatus
{
    Draft,
    Published,
    EntriesOpen,
    EntriesClosed,
    InProgress,
    Completed,
    Cancelled
}

public class Event : SoftDeletableTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? VenueName { get; set; }
    public string? VenueAddress { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public DateOnly EntryDeadline { get; set; }
    public decimal BaseEntryFee { get; set; }
    public string Currency { get; set; } = "GBP";

    public ICollection<EventSession> Sessions { get; set; } = new List<EventSession>();
    public ICollection<Division> Divisions { get; set; } = new List<Division>();
    public ICollection<EventEntry> Entries { get; set; } = new List<EventEntry>();
}
