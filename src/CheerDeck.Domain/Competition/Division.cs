namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Common;

public enum ScoresheetType
{
    USS,
    IASF,
    ICUAdaptive
}

public class Division : TenantEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public CheerLevel Level { get; set; }
    public Guid? AgeGridId { get; set; }
    public AgeGrid? AgeGrid { get; set; }
    public ScoresheetType ScoresheetType { get; set; } = ScoresheetType.USS;
    public int? MinTeamSize { get; set; }
    public int? MaxTeamSize { get; set; }
    public decimal? EntryFeeOverride { get; set; }
    public int SortOrder { get; set; }

    public ICollection<EventEntry> Entries { get; set; } = new List<EventEntry>();
    public ICollection<ScoresheetTemplate> ScoresheetTemplates { get; set; } = new List<ScoresheetTemplate>();
}
