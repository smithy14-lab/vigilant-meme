namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.Competition;
using Microsoft.EntityFrameworkCore;

public class RunningOrderService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<RunningOrderEntry>> GetForBlockAsync(Guid blockId, CancellationToken ct = default)
    {
        return await db.RunningOrderEntries
            .Include(r => r.Entry).ThenInclude(e => e.Team)
            .Include(r => r.Entry).ThenInclude(e => e.Division)
            .Include(r => r.WarmUpSlot)
            .Where(r => r.BlockId == blockId)
            .OrderBy(r => r.Position)
            .ToListAsync(ct);
    }

    public async Task<RunningOrderEntry> AddToRunningOrderAsync(
        Guid blockId, Guid entryId, int position, TimeOnly? scheduledTime = null, CancellationToken ct = default)
    {
        var roe = new RunningOrderEntry
        {
            TenantId = tenant.TenantId,
            BlockId = blockId,
            EntryId = entryId,
            Position = position,
            ScheduledTime = scheduledTime
        };
        db.RunningOrderEntries.Add(roe);
        await db.SaveChangesAsync(ct);
        return roe;
    }

    public async Task UpdateStatusAsync(Guid runningOrderEntryId, PerformanceStatus status, CancellationToken ct = default)
    {
        var entry = await db.RunningOrderEntries.FindAsync([runningOrderEntryId], ct)
            ?? throw new InvalidOperationException("Running order entry not found");

        entry.Status = status;
        if (status == PerformanceStatus.Performing)
            entry.ActualStartTime = DateTime.UtcNow;
        else if (status == PerformanceStatus.Completed)
            entry.ActualEndTime = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task ReorderAsync(Guid blockId, List<Guid> orderedEntryIds, CancellationToken ct = default)
    {
        var entries = await db.RunningOrderEntries
            .Where(r => r.BlockId == blockId)
            .ToListAsync(ct);

        for (int i = 0; i < orderedEntryIds.Count; i++)
        {
            var entry = entries.FirstOrDefault(e => e.EntryId == orderedEntryIds[i]);
            if (entry != null)
                entry.Position = i + 1;
        }

        await db.SaveChangesAsync(ct);
    }

    public async Task AssignWarmUpAsync(Guid runningOrderEntryId, TimeOnly start, TimeOnly end, string? area, CancellationToken ct = default)
    {
        var existing = await db.WarmUpSlots.FirstOrDefaultAsync(w => w.RunningOrderEntryId == runningOrderEntryId, ct);
        if (existing != null)
        {
            existing.StartTime = start;
            existing.EndTime = end;
            existing.Area = area;
        }
        else
        {
            db.WarmUpSlots.Add(new WarmUpSlot
            {
                TenantId = tenant.TenantId,
                RunningOrderEntryId = runningOrderEntryId,
                StartTime = start,
                EndTime = end,
                Area = area
            });
        }
        await db.SaveChangesAsync(ct);
    }
}
