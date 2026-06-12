namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class ClassService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Class>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Classes
            .Include(c => c.Venue)
            .Include(c => c.Term)
            .Include(c => c.Coaches).ThenInclude(cc => cc.Coach)
            .Include(c => c.Enrolments)
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.DayOfWeek).ThenBy(c => c.StartTime)
            .ToListAsync(ct);
    }

    public async Task<Class?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Classes
            .Include(c => c.Venue)
            .Include(c => c.Term)
            .Include(c => c.Coaches).ThenInclude(cc => cc.Coach)
            .Include(c => c.Enrolments).ThenInclude(e => e.Athlete)
            .Include(c => c.Sessions).ThenInclude(s => s.AttendanceRecords)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }

    public async Task<Class> CreateAsync(Class cls, CancellationToken ct = default)
    {
        cls.TenantId = tenant.TenantId;
        cls.CreatedBy = tenant.UserId;
        db.Classes.Add(cls);
        await db.SaveChangesAsync(ct);
        return cls;
    }

    public async Task<Enrolment> EnrolAthleteAsync(Guid classId, Guid athleteId, CancellationToken ct = default)
    {
        var cls = await db.Classes.Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.Id == classId, ct)
            ?? throw new InvalidOperationException("Class not found");

        var activeCount = cls.Enrolments.Count(e => e.Status == EnrolmentStatus.Active);

        var enrolment = new Enrolment
        {
            TenantId = tenant.TenantId,
            AthleteId = athleteId,
            ClassId = classId,
            EnrolledDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = activeCount >= cls.Capacity ? EnrolmentStatus.WaitingList : EnrolmentStatus.Active,
            WaitingListPosition = activeCount >= cls.Capacity
                ? cls.Enrolments.Count(e => e.Status == EnrolmentStatus.WaitingList) + 1
                : null
        };

        db.Enrolments.Add(enrolment);
        await db.SaveChangesAsync(ct);
        return enrolment;
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

    public async Task MarkAttendanceAsync(Guid sessionId, Guid athleteId, AttendanceStatus status, CancellationToken ct = default)
    {
        var existing = await db.AttendanceRecords
            .FirstOrDefaultAsync(a => a.ClassSessionId == sessionId && a.AthleteId == athleteId, ct);

        if (existing != null)
        {
            existing.Status = status;
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
                MarkedAt = DateTime.UtcNow,
                MarkedBy = tenant.UserId
            });
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<Class>> GetAvailableClassesAsync(CancellationToken ct = default)
    {
        return await db.Classes
            .Include(c => c.Venue)
            .Include(c => c.Term)
            .Include(c => c.Enrolments)
            .Where(c => !c.IsDeleted && c.IsActive
                && c.Term != null && c.Term.IsActive
                && c.Enrolments.Count(e => e.Status == EnrolmentStatus.Active) < c.Capacity)
            .OrderBy(c => c.DayOfWeek).ThenBy(c => c.StartTime)
            .ToListAsync(ct);
    }
}
