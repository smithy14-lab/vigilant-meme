namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.Common;

public enum AwardType
{
    FirstPlace,
    SecondPlace,
    ThirdPlace,
    GrandChampion,
    SpecialAward
}

public class Award : TenantEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public Guid DivisionId { get; set; }
    public Division Division { get; set; } = null!;
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public AwardType Type { get; set; }
    public string? CustomName { get; set; }
    public bool IsAnnounced { get; set; }
}
