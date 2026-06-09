namespace CheerDeck.Domain.ClubManagement;

using CheerDeck.Domain.Common;

public enum ChatRoomType
{
    Club,
    Class,
    Team,
    DirectMessage,
    CoachesOnly
}

public class ChatRoom : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public ChatRoomType Type { get; set; }
    public Guid? ClassId { get; set; }
    public Class? Class { get; set; }
    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();
}

public class ChatRoomMember : TenantEntity
{
    public Guid ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }
    public bool IsMuted { get; set; }
}

public class ChatMessage : TenantEntity
{
    public Guid ChatRoomId { get; set; }
    public ChatRoom ChatRoom { get; set; } = null!;
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
}
