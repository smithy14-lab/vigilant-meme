namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class TermService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Term>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Terms.OrderByDescending(t => t.StartDate).ToListAsync(ct);
    }

    public async Task<Term?> GetActiveAsync(CancellationToken ct = default)
    {
        return await db.Terms.FirstOrDefaultAsync(t => t.IsActive, ct);
    }

    public async Task<Term> CreateAsync(Term term, CancellationToken ct = default)
    {
        term.TenantId = tenant.TenantId;
        term.CreatedBy = tenant.UserId;
        db.Terms.Add(term);
        await db.SaveChangesAsync(ct);
        return term;
    }

    public async Task<Term> DuplicateAsync(Guid termId, string newName, DateOnly newStart, DateOnly newEnd, CancellationToken ct = default)
    {
        var source = await db.Terms
            .Include(t => t.Classes.Where(c => !c.IsDeleted))
            .ThenInclude(c => c.Coaches)
            .FirstOrDefaultAsync(t => t.Id == termId, ct)
            ?? throw new InvalidOperationException("Term not found");

        var newTerm = new Term
        {
            TenantId = tenant.TenantId,
            Name = newName,
            StartDate = newStart,
            EndDate = newEnd,
            CreatedBy = tenant.UserId
        };
        db.Terms.Add(newTerm);

        foreach (var cls in source.Classes)
        {
            var newClass = new Class
            {
                TenantId = tenant.TenantId,
                Name = cls.Name,
                TermId = newTerm.Id,
                VenueId = cls.VenueId,
                DayOfWeek = cls.DayOfWeek,
                StartTime = cls.StartTime,
                EndTime = cls.EndTime,
                Capacity = cls.Capacity,
                Level = cls.Level,
                PricePerSession = cls.PricePerSession,
                TermPrice = cls.TermPrice,
                CreatedBy = tenant.UserId
            };
            db.Classes.Add(newClass);

            foreach (var coach in cls.Coaches)
            {
                db.ClassCoaches.Add(new ClassCoach
                {
                    TenantId = tenant.TenantId,
                    ClassId = newClass.Id,
                    CoachId = coach.CoachId,
                    IsLead = coach.IsLead
                });
            }
        }

        await db.SaveChangesAsync(ct);
        return newTerm;
    }
}
