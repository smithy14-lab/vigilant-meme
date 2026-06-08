namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class CampService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Camp>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Camps
            .Include(c => c.Bookings).ThenInclude(b => b.Athlete)
            .Include(c => c.Venue)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<Camp> CreateAsync(Camp camp, CancellationToken ct = default)
    {
        camp.TenantId = tenant.TenantId;
        camp.CreatedBy = tenant.UserId;
        db.Camps.Add(camp);
        await db.SaveChangesAsync(ct);
        return camp;
    }

    public async Task<CampBooking> BookAthleteAsync(Guid campId, Guid athleteId, CancellationToken ct = default)
    {
        var camp = await db.Camps.Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == campId, ct)
            ?? throw new InvalidOperationException("Camp not found");

        var booking = new CampBooking
        {
            TenantId = tenant.TenantId,
            CampId = campId,
            AthleteId = athleteId,
            Status = camp.IsFull ? CampBookingStatus.WaitingList : CampBookingStatus.Confirmed,
            WaitingListPosition = camp.IsFull ? camp.WaitingCount + 1 : null
        };

        db.CampBookings.Add(booking);
        await db.SaveChangesAsync(ct);
        return booking;
    }

    public async Task ProcessWaitingListAsync(Guid campId, CancellationToken ct = default)
    {
        var camp = await db.Camps.Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == campId, ct)
            ?? throw new InvalidOperationException("Camp not found");

        while (!camp.IsFull)
        {
            var next = camp.Bookings
                .Where(b => b.Status == CampBookingStatus.WaitingList)
                .OrderBy(b => b.WaitingListPosition)
                .FirstOrDefault();
            if (next is null) break;

            next.Status = CampBookingStatus.Invited;
            next.InvitedAt = DateTime.UtcNow;
            next.InviteExpiresAt = DateTime.UtcNow.AddHours(48);
            next.WaitingListPosition = null;
        }

        await db.SaveChangesAsync(ct);
    }
}
