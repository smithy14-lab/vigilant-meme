namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum CheerLevel
{
    Novice = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    Level6 = 6,
    Level7 = 7
}

public class Athlete : SoftDeletableTenantEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public CheerLevel Level { get; set; }
    public string? MedicalNotes { get; set; }
    public bool HasMediaConsent { get; set; }
    public bool HasMedicalConsent { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? ExternalMembershipId { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";

    public int GetAgeOnDate(DateOnly date)
    {
        int age = date.Year - DateOfBirth.Year;
        if (DateOfBirth > date.AddYears(-age)) age--;
        return age;
    }

    public ICollection<AthleteGuardian> Guardians { get; set; } = new List<AthleteGuardian>();
    public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<AthleteCrossover> Crossovers { get; set; } = new List<AthleteCrossover>();
}

public class AthleteCrossover : TenantEntity
{
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public string CrossoverClubName { get; set; } = string.Empty;
    public Guid? CrossoverTenantId { get; set; }
    public CheerLevel CrossoverLevel { get; set; }
    public string? Notes { get; set; }
}
