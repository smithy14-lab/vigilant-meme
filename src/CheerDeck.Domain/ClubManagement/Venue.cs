namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public class Venue : SoftDeletableTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Postcode { get; set; }
    public int? DefaultCapacity { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
