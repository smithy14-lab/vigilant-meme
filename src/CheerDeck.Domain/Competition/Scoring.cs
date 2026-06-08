namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.Common;

public class ScoresheetTemplate : TenantEntity
{
    public Guid DivisionId { get; set; }
    public Division Division { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public ScoresheetType Type { get; set; }

    public ICollection<ScoresheetCaption> Captions { get; set; } = new List<ScoresheetCaption>();
}

public class ScoresheetCaption : BaseEntity
{
    public Guid TemplateId { get; set; }
    public ScoresheetTemplate Template { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; } = 1.0m;
    public int SortOrder { get; set; }

    public ICollection<ScoresheetSubcaption> Subcaptions { get; set; } = new List<ScoresheetSubcaption>();
}

public class ScoresheetSubcaption : BaseEntity
{
    public Guid CaptionId { get; set; }
    public ScoresheetCaption Caption { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal MinScore { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Increment { get; set; } = 0.1m;
    public decimal Weight { get; set; } = 1.0m;
    public int SortOrder { get; set; }
}

public class JudgePanel : TenantEntity
{
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public string Name { get; set; } = string.Empty;

    public ICollection<JudgePanelMember> Members { get; set; } = new List<JudgePanelMember>();
}

public class JudgePanelMember : TenantEntity
{
    public Guid PanelId { get; set; }
    public JudgePanel Panel { get; set; } = null!;
    public string JudgeUserId { get; set; } = string.Empty;
    public string JudgeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? AssignedCaptionIndex { get; set; }
}

public class Score : TenantEntity
{
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public Guid PanelMemberId { get; set; }
    public JudgePanelMember PanelMember { get; set; } = null!;
    public Guid SubcaptionId { get; set; }
    public ScoresheetSubcaption Subcaption { get; set; } = null!;
    public decimal Value { get; set; }
    public bool IsOffline { get; set; }
    public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
    public DateTime? SyncedAt { get; set; }
    public string? ClientId { get; set; }
    public long Version { get; set; }
}

public class Deduction : TenantEntity
{
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Points { get; set; }
    public string AssessedByUserId { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}

public class TabulatedResult : TenantEntity
{
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public Guid DivisionId { get; set; }
    public Division Division { get; set; } = null!;
    public decimal RawScore { get; set; }
    public decimal DeductionTotal { get; set; }
    public decimal FinalScore { get; set; }
    public int Rank { get; set; }
    public bool IsReleased { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

public class ScoreCheckRequest : TenantEntity
{
    public Guid EntryId { get; set; }
    public EventEntry Entry { get; set; } = null!;
    public Guid ClubTenantId { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Resolution { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
