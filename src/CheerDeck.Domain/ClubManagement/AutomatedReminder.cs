namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum ReminderType
{
    ClassReminder,
    PaymentDue,
    WaiverExpiring,
    CampBookingConfirmation,
    TermRenewal,
    AttendanceFollowUp,
    Custom
}

public enum ReminderChannel
{
    InApp,
    Email,
    Both
}

public enum ReminderStatus
{
    Active,
    Paused,
    Completed,
    Failed
}

public class AutomatedReminder : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public ReminderType Type { get; set; }
    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int DaysBefore { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurringIntervalDays { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Active;

    public ICollection<ReminderLog> Logs { get; set; } = new List<ReminderLog>();
}

public class ReminderLog : TenantEntity
{
    public Guid ReminderId { get; set; }
    public AutomatedReminder Reminder { get; set; } = null!;
    public Guid? GuardianId { get; set; }
    public Guardian? Guardian { get; set; }
    public string? RecipientEmail { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool Delivered { get; set; }
    public string? Error { get; set; }
}
