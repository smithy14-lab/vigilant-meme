namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.Common;

public enum PerformanceStatus
{
    Scheduled,
    InWarmUp,
    OnDeck,
    OnFloor,
    Performing,
    Completed,
    Scratched
}

public class RunningOrderEntry : TenantEntity
{
    public Guid BlockId { get; set; }
    public SessionBlock Block { get; set; } = null!;
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public int Position { get; set; }
    public TimeOnly? ScheduledTime { get; set; }
    public PerformanceStatus Status { get; set; } = PerformanceStatus.Scheduled;
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }

    public WarmUpSlot? WarmUpSlot { get; set; }
}

public class WarmUpSlot : TenantEntity
{
    public Guid RunningOrderEntryId { get; set; }
    public RunningOrderEntry RunningOrderEntry { get; set; } = null!;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? Area { get; set; }
}
