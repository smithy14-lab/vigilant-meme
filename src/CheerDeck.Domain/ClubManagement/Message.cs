namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum MessageScope
{
    Club,
    Class,
    Individual
}

public class Message : TenantEntity
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageScope Scope { get; set; }
    public Guid? ClassId { get; set; }
    public Class? Class { get; set; }
    public string SentByUserId { get; set; } = string.Empty;
    public string? SentByName { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool SendEmail { get; set; }

    public ICollection<MessageRecipient> Recipients { get; set; } = new List<MessageRecipient>();
}

public class MessageRecipient : TenantEntity
{
    public Guid MessageId { get; set; }
    public Message Message { get; set; } = null!;
    public Guid? GuardianId { get; set; }
    public Guardian? Guardian { get; set; }
    public Guid? CoachId { get; set; }
    public Coach? Coach { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
