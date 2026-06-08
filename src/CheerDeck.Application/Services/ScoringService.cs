namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.Competition;
using Microsoft.EntityFrameworkCore;

public class ScoringService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<Score> SubmitScoreAsync(Score score, CancellationToken ct = default)
    {
        score.TenantId = tenant.TenantId;

        var existing = await db.Scores
            .FirstOrDefaultAsync(s =>
                s.EntryId == score.EntryId &&
                s.PanelMemberId == score.PanelMemberId &&
                s.SubcaptionId == score.SubcaptionId, ct);

        if (existing != null)
        {
            if (score.Version <= existing.Version) return existing;
            existing.Value = score.Value;
            existing.ScoredAt = score.ScoredAt;
            existing.Version = score.Version;
            existing.IsOffline = score.IsOffline;
            existing.SyncedAt = score.IsOffline ? DateTime.UtcNow : null;
        }
        else
        {
            if (score.IsOffline)
                score.SyncedAt = DateTime.UtcNow;
            db.Scores.Add(score);
        }

        await db.SaveChangesAsync(ct);
        return score;
    }

    public async Task SyncOfflineScoresAsync(List<Score> scores, CancellationToken ct = default)
    {
        foreach (var score in scores)
        {
            score.IsOffline = true;
            await SubmitScoreAsync(score, ct);
        }
    }

    public async Task<Deduction> AddDeductionAsync(Deduction deduction, CancellationToken ct = default)
    {
        deduction.TenantId = tenant.TenantId;
        db.Deductions.Add(deduction);
        await db.SaveChangesAsync(ct);
        return deduction;
    }

    public async Task<TabulatedResult> TabulateAsync(Guid entryId, Guid divisionId, CancellationToken ct = default)
    {
        var scores = await db.Scores
            .Include(s => s.Subcaption).ThenInclude(sc => sc.Caption)
            .Where(s => s.EntryId == entryId)
            .ToListAsync(ct);

        var deductions = await db.Deductions
            .Where(d => d.EntryId == entryId)
            .ToListAsync(ct);

        decimal rawScore = 0;
        foreach (var score in scores)
        {
            rawScore += score.Value * score.Subcaption.Weight * score.Subcaption.Caption.Weight;
        }

        decimal deductionTotal = deductions.Sum(d => d.Points);
        decimal finalScore = Math.Max(0, rawScore - deductionTotal);

        var existing = await db.TabulatedResults
            .FirstOrDefaultAsync(t => t.EntryId == entryId && t.DivisionId == divisionId, ct);

        if (existing != null)
        {
            existing.RawScore = rawScore;
            existing.DeductionTotal = deductionTotal;
            existing.FinalScore = finalScore;
            existing.CalculatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
            return existing;
        }

        var result = new TabulatedResult
        {
            TenantId = tenant.TenantId,
            EntryId = entryId,
            DivisionId = divisionId,
            RawScore = rawScore,
            DeductionTotal = deductionTotal,
            FinalScore = finalScore
        };

        db.TabulatedResults.Add(result);
        await db.SaveChangesAsync(ct);
        return result;
    }

    public async Task RankDivisionAsync(Guid divisionId, CancellationToken ct = default)
    {
        var results = await db.TabulatedResults
            .Where(t => t.DivisionId == divisionId)
            .OrderByDescending(t => t.FinalScore)
            .ToListAsync(ct);

        for (int i = 0; i < results.Count; i++)
            results[i].Rank = i + 1;

        await db.SaveChangesAsync(ct);
    }

    public async Task ReleaseScoresAsync(Guid divisionId, CancellationToken ct = default)
    {
        var results = await db.TabulatedResults
            .Where(t => t.DivisionId == divisionId)
            .ToListAsync(ct);

        foreach (var r in results)
        {
            r.IsReleased = true;
            r.ReleasedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task<List<TabulatedResult>> GetResultsAsync(Guid divisionId, bool releasedOnly = false, CancellationToken ct = default)
    {
        var query = db.TabulatedResults
            .Include(t => t.Entry).ThenInclude(e => e.Team)
            .Where(t => t.DivisionId == divisionId);

        if (releasedOnly)
            query = query.Where(t => t.IsReleased);

        return await query.OrderBy(t => t.Rank).ToListAsync(ct);
    }
}
