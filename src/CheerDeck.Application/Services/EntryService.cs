namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Competition;
using CheerDeck.Domain.Integration;
using Microsoft.EntityFrameworkCore;

public class EntryService(
    IAppDbContext db,
    ITenantContext tenant,
    IEligibilityProvider eligibility,
    IMusicLicenceProvider musicLicence,
    IPaymentGateway payments)
{
    public async Task<EventEntry> SubmitEntryAsync(
        Guid eventId, Guid divisionId, Guid teamId, Guid? musicFileId,
        CancellationToken ct = default)
    {
        var team = await db.Teams
            .Include(t => t.Members).ThenInclude(m => m.Athlete)
            .Include(t => t.HeadCoach)
            .Include(t => t.Music)
            .FirstOrDefaultAsync(t => t.Id == teamId, ct)
            ?? throw new InvalidOperationException("Team not found");

        var division = await db.Divisions
            .Include(d => d.Event)
            .FirstOrDefaultAsync(d => d.Id == divisionId, ct)
            ?? throw new InvalidOperationException("Division not found");

        var athletes = team.Members
            .Select(m => (m.AthleteId, m.Athlete.ExternalMembershipId, m.Athlete.DateOfBirth))
            .ToList();

        var eligResult = await eligibility.ValidateTeamEntryAsync(
            athletes,
            (team.HeadCoach?.Id ?? Guid.Empty, team.HeadCoach?.Qualifications?.FirstOrDefault()?.ExternalCredentialId),
            division.Name, division.Level.ToString(), ct);

        var entry = new EventEntry
        {
            TenantId = division.TenantId,
            EventId = eventId,
            DivisionId = divisionId,
            ClubTenantId = tenant.TenantId,
            TeamId = teamId,
            EligibilityPassed = eligResult.IsEligible,
            EligibilityNotes = eligResult.Reason,
            EntryFee = division.EntryFeeOverride ?? division.Event.BaseEntryFee,
            Status = eligResult.IsEligible ? EntryStatus.EligibilityChecked : EntryStatus.Rejected
        };

        foreach (var member in team.Members)
        {
            var athleteResult = await eligibility.CheckAthleteEligibilityAsync(
                member.AthleteId, member.Athlete.ExternalMembershipId,
                member.Athlete.DateOfBirth, division.Name, ct);

            entry.Athletes.Add(new EntryAthlete
            {
                TenantId = division.TenantId,
                AthleteId = member.AthleteId,
                IsAlternate = member.IsAlternate,
                EligibilityPassed = athleteResult.IsEligible,
                EligibilityNotes = athleteResult.Reason
            });
        }

        if (musicFileId.HasValue)
        {
            var music = team.Music.FirstOrDefault(m => m.Id == musicFileId.Value);
            if (music != null)
            {
                entry.MusicFileId = music.Id;
                if (!string.IsNullOrEmpty(music.LicenceProof))
                {
                    var licResult = await musicLicence.VerifyLicenceAsync(music.LicenceProof, music.FileName, ct);
                    entry.MusicLicenceVerified = licResult.IsValid;
                }
            }
        }

        db.EventEntries.Add(entry);
        await db.SaveChangesAsync(ct);

        if (entry.EligibilityPassed)
        {
            var intent = await payments.CreatePaymentIntentAsync(
                entry.EntryFee, division.Event.Currency,
                $"Entry: {team.Name} - {division.Name}",
                new Dictionary<string, string>
                {
                    ["entryId"] = entry.Id.ToString(),
                    ["eventId"] = eventId.ToString()
                }, ct);

            entry.PaymentId = intent.Id;
            entry.Status = EntryStatus.PaymentPending;
            await db.SaveChangesAsync(ct);
        }

        return entry;
    }

    public async Task ConfirmPaymentAsync(Guid entryId, string paymentId, CancellationToken ct = default)
    {
        var entry = await db.EventEntries.FindAsync([entryId], ct)
            ?? throw new InvalidOperationException("Entry not found");
        entry.Status = EntryStatus.Confirmed;
        entry.PaymentId = paymentId;
        entry.PaidAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<EventEntry>> GetEntriesForEventAsync(Guid eventId, CancellationToken ct = default)
    {
        return await db.EventEntries
            .Include(e => e.Team)
            .Include(e => e.Division)
            .Include(e => e.Athletes).ThenInclude(a => a.Athlete)
            .Where(e => e.EventId == eventId)
            .OrderBy(e => e.Division.SortOrder)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateEntryNumberAsync(Guid eventId, Guid divisionId, CancellationToken ct = default)
    {
        var evt = await db.Events.FindAsync([eventId], ct);
        var division = await db.Divisions.FindAsync([divisionId], ct);

        // Build event slug: take first letters/numbers, uppercase, max 4 chars
        var eventSlug = new string(
            (evt?.Name ?? "EVT")
                .ToUpperInvariant()
                .Where(c => char.IsLetterOrDigit(c))
                .Take(4)
                .ToArray());
        if (string.IsNullOrEmpty(eventSlug)) eventSlug = "EVT";

        // Build division abbreviation: first letter + level digit(s), max 3 chars
        var divName = division?.Name ?? "DIV";
        var levelNum = division?.Level.ToString("D") ?? "0";
        var divAbbr = (divName.Length > 0 ? divName[0].ToString().ToUpperInvariant() : "D")
                      + "L" + levelNum;
        if (divAbbr.Length > 4) divAbbr = divAbbr[..4];

        // Count existing entries for this event+division
        var existingCount = await db.EventEntries
            .CountAsync(e => e.EventId == eventId && e.DivisionId == divisionId, ct);

        var sequence = (existingCount + 1).ToString("D3");
        return $"{eventSlug}-{divAbbr}-{sequence}";
    }

    public async Task<EventEntry> CreateAsync(EventEntry entry, CancellationToken ct = default)
    {
        entry.TenantId = tenant.TenantId;
        entry.SubmittedAt = DateTime.UtcNow;

        // Generate entry number
        entry.EntryNumber = await GenerateEntryNumberAsync(entry.EventId, entry.DivisionId, ct);

        // Determine fee
        var division = await db.Divisions
            .Include(d => d.Event)
            .FirstOrDefaultAsync(d => d.Id == entry.DivisionId, ct);
        if (division is not null)
        {
            entry.EntryFee = division.EntryFeeOverride ?? division.Event.BaseEntryFee;
        }

        entry.Status = EntryStatus.PaymentPending;
        entry.EligibilityPassed = true; // simplified for public entry flow

        db.EventEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return entry;
    }
}
