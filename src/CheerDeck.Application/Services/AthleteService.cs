namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class AthleteService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Athlete>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Athletes
            .Include(a => a.Guardians).ThenInclude(ag => ag.Guardian)
            .Where(a => !a.IsDeleted && a.IsActive)
            .OrderBy(a => a.LastName).ThenBy(a => a.FirstName)
            .ToListAsync(ct);
    }

    public async Task<Athlete?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Athletes
            .Include(a => a.Guardians).ThenInclude(ag => ag.Guardian)
            .Include(a => a.TeamMemberships).ThenInclude(tm => tm.Team)
            .Include(a => a.Crossovers)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, ct);
    }

    public async Task<Athlete> CreateAsync(Athlete athlete, CancellationToken ct = default)
    {
        athlete.TenantId = tenant.TenantId;
        athlete.CreatedBy = tenant.UserId;
        db.Athletes.Add(athlete);
        await db.SaveChangesAsync(ct);
        return athlete;
    }

    public async Task<Athlete> UpdateAsync(Athlete athlete, CancellationToken ct = default)
    {
        athlete.UpdatedAt = DateTime.UtcNow;
        athlete.UpdatedBy = tenant.UserId;
        await db.SaveChangesAsync(ct);
        return athlete;
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var athlete = await db.Athletes.FindAsync([id], ct);
        if (athlete is null) return;
        athlete.IsDeleted = true;
        athlete.DeletedAt = DateTime.UtcNow;
        athlete.DeletedBy = tenant.UserId;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<Athlete>> SearchAsync(string query, CancellationToken ct = default)
    {
        return await db.Athletes
            .Where(a => !a.IsDeleted && a.IsActive &&
                        (a.FirstName.Contains(query) || a.LastName.Contains(query)))
            .OrderBy(a => a.LastName)
            .Take(50)
            .ToListAsync(ct);
    }
}
