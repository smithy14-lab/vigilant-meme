namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.Competition;
using Microsoft.EntityFrameworkCore;

public class EventService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Event>> GetAllAsync(CancellationToken ct = default)
    {
        var events = await db.Events
            .Include(e => e.Sessions)
            .Include(e => e.Divisions)
            .Where(e => !e.IsDeleted)
            .ToListAsync(ct);

        return events
            .OrderByDescending(e => e.Sessions.Any() ? e.Sessions.Min(s => s.Date) : DateOnly.MinValue)
            .ToList();
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Events
            .Include(e => e.Sessions).ThenInclude(s => s.Blocks)
            .Include(e => e.Divisions).ThenInclude(d => d.AgeGrid)
            .Include(e => e.Entries).ThenInclude(en => en.Team)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);
    }

    public async Task<Event> CreateAsync(Event evt, CancellationToken ct = default)
    {
        evt.TenantId = tenant.TenantId;
        evt.CreatedBy = tenant.UserId;
        db.Events.Add(evt);
        await db.SaveChangesAsync(ct);
        return evt;
    }

    public async Task<Division> AddDivisionAsync(Guid eventId, Division division, CancellationToken ct = default)
    {
        division.EventId = eventId;
        division.TenantId = tenant.TenantId;
        db.Divisions.Add(division);
        await db.SaveChangesAsync(ct);
        return division;
    }

    public async Task<EventSession> AddSessionAsync(Guid eventId, EventSession session, CancellationToken ct = default)
    {
        session.EventId = eventId;
        session.TenantId = tenant.TenantId;
        db.EventSessions.Add(session);
        await db.SaveChangesAsync(ct);
        return session;
    }
}
