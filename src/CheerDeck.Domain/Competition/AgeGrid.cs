namespace CheerDeck.Domain.Competition;

using CheerDeck.Domain.Common;

public class AgeGrid : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Provider { get; set; } = "SportCheerUK";
    public int Season { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<AgeGridDivision> Divisions { get; set; } = new List<AgeGridDivision>();
}

public class AgeGridDivision : BaseEntity
{
    public Guid AgeGridId { get; set; }
    public AgeGrid AgeGrid { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int MinAge { get; set; }
    public int MaxAge { get; set; }
    public DateOnly? AgeCalculationDate { get; set; }
}
