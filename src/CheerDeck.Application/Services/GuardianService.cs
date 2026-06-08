namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class GuardianService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Guardian>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Guardians
            .Include(g => g.Athletes).ThenInclude(ag => ag.Athlete)
            .Where(g => !g.IsDeleted && g.IsActive)
            .OrderBy(g => g.LastName)
            .ToListAsync(ct);
    }

    public async Task<Guardian?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Guardians
            .Include(g => g.Athletes).ThenInclude(ag => ag.Athlete)
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted, ct);
    }

    public async Task<Guardian> CreateAsync(Guardian guardian, CancellationToken ct = default)
    {
        guardian.TenantId = tenant.TenantId;
        guardian.CreatedBy = tenant.UserId;
        db.Guardians.Add(guardian);
        await db.SaveChangesAsync(ct);
        return guardian;
    }

    public async Task LinkAthleteAsync(Guid guardianId, Guid athleteId, string relationship, bool isPrimary = false, CancellationToken ct = default)
    {
        db.AthleteGuardians.Add(new AthleteGuardian
        {
            TenantId = tenant.TenantId,
            GuardianId = guardianId,
            AthleteId = athleteId,
            Relationship = relationship,
            IsPrimaryContact = isPrimary
        });
        await db.SaveChangesAsync(ct);
    }
}
