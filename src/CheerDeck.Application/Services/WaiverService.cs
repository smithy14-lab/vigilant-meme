namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class WaiverService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Waiver>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Waivers
            .Include(w => w.Signatures)
            .OrderBy(w => w.Type)
            .ThenByDescending(w => w.Version)
            .ToListAsync(ct);
    }

    public async Task<List<Waiver>> GetActiveAsync(CancellationToken ct = default)
    {
        return await db.Waivers
            .Where(w => w.Status == WaiverStatus.Active)
            .OrderBy(w => w.Type)
            .ToListAsync(ct);
    }

    public async Task<Waiver?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Waivers
            .Include(w => w.Signatures).ThenInclude(s => s.Guardian)
            .Include(w => w.Signatures).ThenInclude(s => s.Athlete)
            .FirstOrDefaultAsync(w => w.Id == id, ct);
    }

    public async Task<Waiver> CreateAsync(Waiver waiver, CancellationToken ct = default)
    {
        waiver.TenantId = tenant.TenantId;
        waiver.CreatedBy = tenant.UserId;
        db.Waivers.Add(waiver);
        await db.SaveChangesAsync(ct);
        return waiver;
    }

    public async Task<WaiverSignature> SignAsync(Guid waiverId, Guid guardianId, Guid? athleteId, string signedByName, string? ipAddress = null, CancellationToken ct = default)
    {
        var signature = new WaiverSignature
        {
            TenantId = tenant.TenantId,
            WaiverId = waiverId,
            GuardianId = guardianId,
            AthleteId = athleteId,
            SignedByName = signedByName,
            IpAddress = ipAddress
        };
        db.WaiverSignatures.Add(signature);
        await db.SaveChangesAsync(ct);
        return signature;
    }

    public async Task<List<Waiver>> GetUnsignedForGuardianAsync(Guid guardianId, CancellationToken ct = default)
    {
        var signedWaiverIds = await db.WaiverSignatures
            .Where(s => s.GuardianId == guardianId && !s.IsRevoked)
            .Select(s => s.WaiverId)
            .Distinct()
            .ToListAsync(ct);

        return await db.Waivers
            .Where(w => w.Status == WaiverStatus.Active && !signedWaiverIds.Contains(w.Id))
            .ToListAsync(ct);
    }

    public async Task<List<WaiverSignature>> GetSignaturesForGuardianAsync(Guid guardianId, CancellationToken ct = default)
    {
        return await db.WaiverSignatures
            .Include(s => s.Waiver)
            .Include(s => s.Athlete)
            .Where(s => s.GuardianId == guardianId && !s.IsRevoked)
            .OrderByDescending(s => s.SignedAt)
            .ToListAsync(ct);
    }
}
