namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class CoachService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Coach>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Coaches
            .Include(c => c.Qualifications)
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.LastName)
            .ToListAsync(ct);
    }

    public async Task<Coach?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Coaches
            .Include(c => c.Qualifications)
            .Include(c => c.ClassAssignments).ThenInclude(ca => ca.Class)
            .Include(c => c.Teams)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }

    public async Task<Coach> CreateAsync(Coach coach, CancellationToken ct = default)
    {
        coach.TenantId = tenant.TenantId;
        coach.CreatedBy = tenant.UserId;
        db.Coaches.Add(coach);
        await db.SaveChangesAsync(ct);
        return coach;
    }

    public async Task<Coach> UpdateAsync(Coach coach, CancellationToken ct = default)
    {
        coach.UpdatedAt = DateTime.UtcNow;
        coach.UpdatedBy = tenant.UserId;
        await db.SaveChangesAsync(ct);
        return coach;
    }

    public async Task<List<Coach>> GetWithExpiringCredentialsAsync(int daysAhead = 30, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysAhead));
        return await db.Coaches
            .Include(c => c.Qualifications)
            .Where(c => !c.IsDeleted && c.IsActive &&
                        c.Qualifications.Any(q => q.ExpiryDate.HasValue && q.ExpiryDate.Value <= cutoff))
            .ToListAsync(ct);
    }
}
