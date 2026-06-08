namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class VenueService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Venue>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Venues
            .Where(v => !v.IsDeleted && v.IsActive)
            .OrderBy(v => v.Name)
            .ToListAsync(ct);
    }

    public async Task<Venue?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Venues
            .Include(v => v.Classes.Where(c => !c.IsDeleted))
            .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted, ct);
    }

    public async Task<Venue> CreateAsync(Venue venue, CancellationToken ct = default)
    {
        venue.TenantId = tenant.TenantId;
        venue.CreatedBy = tenant.UserId;
        db.Venues.Add(venue);
        await db.SaveChangesAsync(ct);
        return venue;
    }
}
