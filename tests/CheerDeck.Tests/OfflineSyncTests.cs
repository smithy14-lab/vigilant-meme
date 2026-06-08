namespace CheerDeck.Tests;

using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Competition;
using CheerDeck.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class OfflineSyncTests
{
    private static readonly Guid TenantId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task Offline_Scores_Are_Synced_And_Marked()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var (entryId, panelMemberId, subcaptionIds) = SetupMinimalScoringData(db);

        var offlineScores = subcaptionIds.Select((id, i) => new Score
        {
            TenantId = TenantId,
            EntryId = entryId,
            PanelMemberId = panelMemberId,
            SubcaptionId = id,
            Value = 7.5m + i * 0.5m,
            IsOffline = true,
            ScoredAt = DateTime.UtcNow.AddMinutes(-5),
            Version = 1
        }).ToList();

        await service.SyncOfflineScoresAsync(offlineScores);

        var saved = await db.Scores.Where(s => s.EntryId == entryId).ToListAsync();
        saved.Should().HaveCount(subcaptionIds.Count);
        saved.Should().AllSatisfy(s =>
        {
            s.IsOffline.Should().BeTrue();
            s.SyncedAt.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task Offline_Score_Does_Not_Overwrite_Newer_Online_Score()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var (entryId, panelMemberId, subcaptionIds) = SetupMinimalScoringData(db);
        var subcaptionId = subcaptionIds.First();

        // Online score arrives first with version 2
        await service.SubmitScoreAsync(new Score
        {
            TenantId = TenantId,
            EntryId = entryId,
            PanelMemberId = panelMemberId,
            SubcaptionId = subcaptionId,
            Value = 9.0m,
            Version = 2
        });

        // Offline score with older version syncs later
        await service.SyncOfflineScoresAsync([new Score
        {
            TenantId = TenantId,
            EntryId = entryId,
            PanelMemberId = panelMemberId,
            SubcaptionId = subcaptionId,
            Value = 6.0m,
            IsOffline = true,
            Version = 1
        }]);

        var score = await db.Scores.FirstAsync(s => s.SubcaptionId == subcaptionId && s.EntryId == entryId);
        score.Value.Should().Be(9.0m);
    }

    private (Guid EntryId, Guid PanelMemberId, List<Guid> SubcaptionIds) SetupMinimalScoringData(AppDbContext db)
    {
        var evt = new Event { TenantId = TenantId, Name = "Sync Test Event", EntryDeadline = DateOnly.MaxValue, BaseEntryFee = 100 };
        db.Events.Add(evt);

        var division = new Division { TenantId = TenantId, EventId = evt.Id, Name = "Test Div", Level = CheerLevel.Level2 };
        db.Divisions.Add(division);

        var template = new ScoresheetTemplate { TenantId = TenantId, DivisionId = division.Id, Name = "Test", Type = ScoresheetType.USS };
        db.ScoresheetTemplates.Add(template);

        var caption = new ScoresheetCaption { TemplateId = template.Id, Name = "Stunts", MaxScore = 10, Weight = 1.0m, SortOrder = 1 };
        db.ScoresheetCaptions.Add(caption);

        var sub1 = new ScoresheetSubcaption { CaptionId = caption.Id, Name = "Difficulty", MinScore = 0, MaxScore = 10, Increment = 0.5m, Weight = 1.0m, SortOrder = 1 };
        var sub2 = new ScoresheetSubcaption { CaptionId = caption.Id, Name = "Execution", MinScore = 0, MaxScore = 10, Increment = 0.5m, Weight = 1.0m, SortOrder = 2 };
        db.ScoresheetSubcaptions.AddRange(sub1, sub2);

        var team = new Team { TenantId = TenantId, Name = "Sync Team", Level = CheerLevel.Level2 };
        db.Teams.Add(team);

        var entry = new EventEntry
        {
            TenantId = TenantId, EventId = evt.Id, DivisionId = division.Id,
            ClubTenantId = TenantId, TeamId = team.Id, Status = EntryStatus.Confirmed,
            EligibilityPassed = true, EntryFee = 100
        };
        db.EventEntries.Add(entry);

        var panel = new JudgePanel { TenantId = TenantId, EventId = evt.Id, Name = "Panel A" };
        db.JudgePanels.Add(panel);
        var member = new JudgePanelMember { TenantId = TenantId, PanelId = panel.Id, JudgeUserId = "j1", JudgeName = "Judge", Role = "Judge" };
        db.JudgePanelMembers.Add(member);

        db.SaveChanges();
        return (entry.Id, member.Id, new List<Guid> { sub1.Id, sub2.Id });
    }
}
