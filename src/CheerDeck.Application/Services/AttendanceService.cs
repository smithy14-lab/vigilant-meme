namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class AttendanceService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<ClassSession>> GetSessionsForClassAsync(Guid classId, CancellationToken ct = default)
    {
        return await db.ClassSessions
            .Include(s => s.AttendanceRecords).ThenInclude(r => r.Athlete)
            .Where(s => s.ClassId == classId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(ct);
    }

    public async Task<ClassSession> CreateSessionAsync(Guid classId, DateOnly date, CancellationToken ct = default)
    {
        var session = new ClassSession
        {
            TenantId = tenant.TenantId,
            ClassId = classId,
            Date = date
        };
        db.ClassSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }

    public async Task MarkAttendanceAsync(Guid sessionId, Guid athleteId, AttendanceStatus status, string? notes = null, CancellationToken ct = default)
    {
        var existing = await db.AttendanceRecords
            .FirstOrDefaultAsync(r => r.ClassSessionId == sessionId && r.AthleteId == athleteId, ct);

        if (existing != null)
        {
            existing.Status = status;
            existing.Notes = notes;
            existing.MarkedAt = DateTime.UtcNow;
            existing.MarkedBy = tenant.UserId;
        }
        else
        {
            db.AttendanceRecords.Add(new AttendanceRecord
            {
                TenantId = tenant.TenantId,
                ClassSessionId = sessionId,
                AthleteId = athleteId,
                Status = status,
                Notes = notes,
                MarkedAt = DateTime.UtcNow,
                MarkedBy = tenant.UserId
            });
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task BulkMarkAttendanceAsync(Guid sessionId, Dictionary<Guid, AttendanceStatus> records, CancellationToken ct = default)
    {
        foreach (var (athleteId, status) in records)
        {
            await MarkAttendanceAsync(sessionId, athleteId, status, ct: ct);
        }
    }

    public async Task CancelSessionAsync(Guid sessionId, string? reason, CancellationToken ct = default)
    {
        var session = await db.ClassSessions.FindAsync([sessionId], ct)
            ?? throw new InvalidOperationException("Session not found");
        session.IsCancelled = true;
        session.CancellationReason = reason;
        await db.SaveChangesAsync(ct);
    }
}
