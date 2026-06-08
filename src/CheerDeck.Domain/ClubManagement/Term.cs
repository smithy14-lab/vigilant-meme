namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public class Term : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
