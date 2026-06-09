namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class ReminderService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<AutomatedReminder>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.AutomatedReminders
            .Include(r => r.Logs)
            .OrderBy(r => r.Type)
            .ToListAsync(ct);
    }

    public async Task<AutomatedReminder> CreateAsync(AutomatedReminder reminder, CancellationToken ct = default)
    {
        reminder.TenantId = tenant.TenantId;
        reminder.CreatedBy = tenant.UserId;
        db.AutomatedReminders.Add(reminder);
        await db.SaveChangesAsync(ct);
        return reminder;
    }

    public async Task UpdateStatusAsync(Guid reminderId, ReminderStatus status, CancellationToken ct = default)
    {
        var reminder = await db.AutomatedReminders.FindAsync([reminderId], ct)
            ?? throw new InvalidOperationException("Reminder not found");
        reminder.Status = status;
        await db.SaveChangesAsync(ct);
    }

    public async Task LogSendAsync(Guid reminderId, Guid? guardianId, string? email, bool delivered, string? error = null, CancellationToken ct = default)
    {
        db.ReminderLogs.Add(new ReminderLog
        {
            TenantId = tenant.TenantId,
            ReminderId = reminderId,
            GuardianId = guardianId,
            RecipientEmail = email,
            Delivered = delivered,
            Error = error
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<ReminderLog>> GetLogsAsync(Guid reminderId, CancellationToken ct = default)
    {
        return await db.ReminderLogs
            .Include(l => l.Guardian)
            .Where(l => l.ReminderId == reminderId)
            .OrderByDescending(l => l.SentAt)
            .Take(100)
            .ToListAsync(ct);
    }
}
