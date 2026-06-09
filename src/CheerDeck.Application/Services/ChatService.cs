namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class ChatService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<ChatRoom>> GetRoomsForUserAsync(string userId, CancellationToken ct = default)
    {
        return await db.ChatRoomMembers
            .Where(m => m.UserId == userId && !m.ChatRoom.IsArchived)
            .Select(m => m.ChatRoom)
            .Include(r => r.Members)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<ChatRoom> CreateRoomAsync(ChatRoom room, CancellationToken ct = default)
    {
        room.TenantId = tenant.TenantId;
        room.CreatedBy = tenant.UserId;
        db.ChatRooms.Add(room);
        await db.SaveChangesAsync(ct);
        return room;
    }

    public async Task<ChatRoomMember> AddMemberAsync(Guid roomId, string userId, string displayName, string role, CancellationToken ct = default)
    {
        var member = new ChatRoomMember
        {
            TenantId = tenant.TenantId,
            ChatRoomId = roomId,
            UserId = userId,
            DisplayName = displayName,
            Role = role
        };
        db.ChatRoomMembers.Add(member);
        await db.SaveChangesAsync(ct);
        return member;
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(Guid roomId, int take = 50, CancellationToken ct = default)
    {
        return await db.ChatMessages
            .Where(m => m.ChatRoomId == roomId && !m.IsDeleted)
            .OrderByDescending(m => m.SentAt)
            .Take(take)
            .OrderBy(m => m.SentAt)
            .ToListAsync(ct);
    }

    public async Task<ChatMessage> SendMessageAsync(Guid roomId, string content, CancellationToken ct = default)
    {
        var message = new ChatMessage
        {
            TenantId = tenant.TenantId,
            ChatRoomId = roomId,
            SenderUserId = tenant.UserId ?? string.Empty,
            Content = content
        };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync(ct);
        return message;
    }

    public async Task MarkReadAsync(Guid roomId, string userId, CancellationToken ct = default)
    {
        var member = await db.ChatRoomMembers
            .FirstOrDefaultAsync(m => m.ChatRoomId == roomId && m.UserId == userId, ct);
        if (member != null)
        {
            member.LastReadAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }
}
