namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class PrivateLessonService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<PrivateLesson>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.PrivateLessons
            .Include(p => p.Coach)
            .Include(p => p.Athletes).ThenInclude(pa => pa.Athlete)
            .Include(p => p.Venue)
            .OrderByDescending(p => p.StartTime)
            .ToListAsync(ct);
    }

    public async Task<PrivateLesson> CreateAsync(PrivateLesson lesson, List<Guid> athleteIds, CancellationToken ct = default)
    {
        lesson.TenantId = tenant.TenantId;
        lesson.CreatedBy = tenant.UserId;
        db.PrivateLessons.Add(lesson);

        foreach (var athleteId in athleteIds)
        {
            db.PrivateLessonAthletes.Add(new PrivateLessonAthlete
            {
                TenantId = tenant.TenantId,
                PrivateLessonId = lesson.Id,
                AthleteId = athleteId
            });
        }

        await db.SaveChangesAsync(ct);
        return lesson;
    }

    public async Task UpdateStatusAsync(Guid lessonId, PrivateLessonStatus status, CancellationToken ct = default)
    {
        var lesson = await db.PrivateLessons.FindAsync([lessonId], ct)
            ?? throw new InvalidOperationException("Lesson not found");
        lesson.Status = status;
        await db.SaveChangesAsync(ct);
    }
}
