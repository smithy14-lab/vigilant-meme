namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public class Guardian : SoftDeletableTenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? UserId { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";

    public ICollection<AthleteGuardian> Athletes { get; set; } = new List<AthleteGuardian>();
}

public class AthleteGuardian : TenantEntity
{
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public Guid GuardianId { get; set; }
    public Guardian Guardian { get; set; } = null!;
    public string Relationship { get; set; } = string.Empty;
    public bool IsPrimaryContact { get; set; }
}
