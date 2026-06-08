namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class MessageService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Message>> GetInboxAsync(Guid? recipientGuardianId = null, CancellationToken ct = default)
    {
        var query = db.Messages
            .Include(m => m.Recipients)
            .OrderByDescending(m => m.SentAt)
            .AsQueryable();

        if (recipientGuardianId.HasValue)
        {
            query = query.Where(m => m.Recipients.Any(r => r.GuardianId == recipientGuardianId));
        }

        return await query.Take(100).ToListAsync(ct);
    }

    public async Task<Message> SendAsync(Message message, List<Guid>? guardianIds = null, CancellationToken ct = default)
    {
        message.TenantId = tenant.TenantId;
        message.SentByUserId = tenant.UserId ?? string.Empty;
        message.CreatedBy = tenant.UserId;

        if (guardianIds != null)
        {
            foreach (var gid in guardianIds)
            {
                message.Recipients.Add(new MessageRecipient
                {
                    TenantId = tenant.TenantId,
                    GuardianId = gid
                });
            }
        }
        else if (message.Scope == MessageScope.Class && message.ClassId.HasValue)
        {
            var guardians = await db.Enrolments
                .Where(e => e.ClassId == message.ClassId && e.Status == EnrolmentStatus.Active)
                .SelectMany(e => e.Athlete.Guardians.Select(ag => ag.GuardianId))
                .Distinct()
                .ToListAsync(ct);

            foreach (var gid in guardians)
            {
                message.Recipients.Add(new MessageRecipient
                {
                    TenantId = tenant.TenantId,
                    GuardianId = gid
                });
            }
        }

        db.Messages.Add(message);
        await db.SaveChangesAsync(ct);
        return message;
    }
}
