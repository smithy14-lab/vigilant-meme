namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.Common;

public class EventSession : TenantEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public int SortOrder { get; set; }

    public ICollection<SessionBlock> Blocks { get; set; } = new List<SessionBlock>();
}

public class SessionBlock : TenantEntity
{
    public Guid SessionId { get; set; }
    public EventSession Session { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public TimeOnly? StartTime { get; set; }
    public int SortOrder { get; set; }

    public ICollection<RunningOrderEntry> RunningOrder { get; set; } = new List<RunningOrderEntry>();
}
