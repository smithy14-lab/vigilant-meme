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

    public async Task<AutoScheduleResult> AutoScheduleAsync(
        Guid eventId,
        int routineMinutes = 3,
        int transitionMinutes = 2,
        int warmUpMinutes = 5,
        string[] warmUpAreas = null!,
        CancellationToken ct = default)
    {
        warmUpAreas ??= ["A", "B"];
        var slotMinutes = routineMinutes + transitionMinutes;

        var entries = await db.EventEntries
            .Include(e => e.Division)
            .Where(e => e.EventId == eventId && (e.Status == EntryStatus.Confirmed || e.Status == EntryStatus.EligibilityChecked))
            .ToListAsync(ct);

        if (entries.Count == 0)
            return new AutoScheduleResult(0, 0, []);

        var blocks = await db.SessionBlocks
            .Include(b => b.Session)
            .Include(b => b.RunningOrder)
            .Where(b => b.Session.EventId == eventId)
            .OrderBy(b => b.Session.SortOrder).ThenBy(b => b.SortOrder)
            .ToListAsync(ct);

        var existingRoe = await db.RunningOrderEntries
            .Where(r => r.Block.Session.EventId == eventId)
            .ToListAsync(ct);
        db.RunningOrderEntries.RemoveRange(existingRoe);

        var existingWarmUps = await db.WarmUpSlots
            .Where(w => existingRoe.Select(r => r.Id).Contains(w.RunningOrderEntryId))
            .ToListAsync(ct);
        db.WarmUpSlots.RemoveRange(existingWarmUps);

        var entriesByDivision = entries
            .GroupBy(e => e.DivisionId)
            .OrderBy(g => g.First().Division.SortOrder)
            .ToList();

        int totalScheduled = 0;
        int totalWarmUps = 0;
        var blockSummaries = new List<string>();
        int blockIndex = 0;

        foreach (var divGroup in entriesByDivision)
        {
            var divEntries = SeparateClubs(divGroup.ToList());
            var block = blockIndex < blocks.Count ? blocks[blockIndex] : null;
            var blockStartTime = block?.StartTime ?? new TimeOnly(9, 0);

            for (int i = 0; i < divEntries.Count; i++)
            {
                var entry = divEntries[i];
                var scheduledTime = blockStartTime.AddMinutes(i * slotMinutes);

                var roe = new RunningOrderEntry
                {
                    TenantId = tenant.TenantId,
                    BlockId = block?.Id ?? blocks.First().Id,
                    EntryId = entry.Id,
                    Position = i + 1,
                    ScheduledTime = scheduledTime,
                    Status = PerformanceStatus.Scheduled
                };
                db.RunningOrderEntries.Add(roe);

                var warmUpArea = warmUpAreas[i % warmUpAreas.Length];
                var warmUpStart = scheduledTime.AddMinutes(-(warmUpMinutes + 2));
                var warmUpEnd = scheduledTime.AddMinutes(-2);

                db.WarmUpSlots.Add(new WarmUpSlot
                {
                    TenantId = tenant.TenantId,
                    RunningOrderEntryId = roe.Id,
                    StartTime = warmUpStart,
                    EndTime = warmUpEnd,
                    Area = warmUpArea
                });

                totalScheduled++;
                totalWarmUps++;
            }

            blockSummaries.Add($"{divGroup.First().Division.Name}: {divEntries.Count} entries");
            blockIndex++;
        }

        await db.SaveChangesAsync(ct);
        return new AutoScheduleResult(totalScheduled, totalWarmUps, blockSummaries);
    }

    private static List<EventEntry> SeparateClubs(List<EventEntry> entries)
    {
        if (entries.Count <= 1) return entries;

        var byClub = entries.GroupBy(e => e.ClubTenantId).ToList();
        if (byClub.Count <= 1) return Shuffle(entries);

        var queues = byClub
            .OrderByDescending(g => g.Count())
            .Select(g => new Queue<EventEntry>(Shuffle(g.ToList())))
            .ToList();

        var result = new List<EventEntry>();
        var lastClub = Guid.Empty;

        while (result.Count < entries.Count)
        {
            var placed = false;
            foreach (var queue in queues.Where(q => q.Count > 0))
            {
                if (queue.Peek().ClubTenantId != lastClub || queues.All(q => q.Count == 0 || q.Peek().ClubTenantId == lastClub))
                {
                    var entry = queue.Dequeue();
                    result.Add(entry);
                    lastClub = entry.ClubTenantId;
                    placed = true;
                    break;
                }
            }
            if (!placed)
            {
                var fallback = queues.First(q => q.Count > 0).Dequeue();
                result.Add(fallback);
                lastClub = fallback.ClubTenantId;
            }
        }

        return result;
    }

    private static List<T> Shuffle<T>(List<T> list)
    {
        var rng = Random.Shared;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }
}

public record AutoScheduleResult(int EntriesScheduled, int WarmUpsAssigned, List<string> BlockSummaries);
