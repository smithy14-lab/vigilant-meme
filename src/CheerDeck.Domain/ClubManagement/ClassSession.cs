namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public class ClassSession : TenantEntity
{
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;
    public DateOnly Date { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    Excused
}

public class AttendanceRecord : TenantEntity
{
    public Guid ClassSessionId { get; set; }
    public ClassSession ClassSession { get; set; } = null!;
    public Guid AthleteId { get; set; }
    public Athlete Athlete { get; set; } = null!;
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? MarkedAt { get; set; }
    public string? MarkedBy { get; set; }
}
