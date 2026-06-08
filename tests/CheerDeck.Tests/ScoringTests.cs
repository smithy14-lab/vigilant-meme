namespace CheerDeck.Tests;

using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Competition;
using CheerDeck.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class ScoringTests
{
    private static readonly Guid TenantId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Tabulate_Calculates_Weighted_Score_Correctly()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var entry = SetupScoringData(db);

        var panelMember = await db.JudgePanelMembers.FirstAsync();
        var subcaptions = await db.ScoresheetSubcaptions.ToListAsync();

        // Submit scores: each subcaption gets a score
        foreach (var sub in subcaptions)
        {
            await service.SubmitScoreAsync(new Score
            {
                TenantId = TenantId,
                EntryId = entry.Id,
                PanelMemberId = panelMember.Id,
                SubcaptionId = sub.Id,
                Value = 8.0m,
                Version = 1
            });
        }

        var result = await service.TabulateAsync(entry.Id, entry.DivisionId);

        result.Should().NotBeNull();
        result.RawScore.Should().BeGreaterThan(0);
        result.DeductionTotal.Should().Be(0);
        result.FinalScore.Should().Be(result.RawScore);
    }

    [Fact]
    public async Task Tabulate_Subtracts_Deductions()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var entry = SetupScoringData(db);
        var panelMember = await db.JudgePanelMembers.FirstAsync();
        var subcaption = await db.ScoresheetSubcaptions.FirstAsync();

        await service.SubmitScoreAsync(new Score
        {
            TenantId = TenantId,
            EntryId = entry.Id,
            PanelMemberId = panelMember.Id,
            SubcaptionId = subcaption.Id,
            Value = 8.0m,
            Version = 1
        });

        await service.AddDeductionAsync(new Deduction
        {
            TenantId = TenantId,
            EntryId = entry.Id,
            Type = "Safety",
            Description = "Illegal stunt",
            Points = 2.0m,
            AssessedByUserId = "judge-1"
        });

        var result = await service.TabulateAsync(entry.Id, entry.DivisionId);

        result.DeductionTotal.Should().Be(2.0m);
        result.FinalScore.Should().Be(result.RawScore - 2.0m);
    }

    [Fact]
    public async Task Score_Version_Prevents_Overwrite_By_Stale_Data()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var entry = SetupScoringData(db);
        var panelMember = await db.JudgePanelMembers.FirstAsync();
        var subcaption = await db.ScoresheetSubcaptions.FirstAsync();

        // V2 arrives first
        await service.SubmitScoreAsync(new Score
        {
            TenantId = TenantId,
            EntryId = entry.Id,
            PanelMemberId = panelMember.Id,
            SubcaptionId = subcaption.Id,
            Value = 9.0m,
            Version = 2
        });

        // V1 arrives late (stale)
        await service.SubmitScoreAsync(new Score
        {
            TenantId = TenantId,
            EntryId = entry.Id,
            PanelMemberId = panelMember.Id,
            SubcaptionId = subcaption.Id,
            Value = 7.0m,
            Version = 1
        });

        var score = await db.Scores.FirstAsync(s =>
            s.EntryId == entry.Id && s.PanelMemberId == panelMember.Id && s.SubcaptionId == subcaption.Id);
        score.Value.Should().Be(9.0m);
        score.Version.Should().Be(2);
    }

    [Fact]
    public async Task RankDivision_Assigns_Correct_Ranks()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var divisionId = Guid.NewGuid();

        db.TabulatedResults.AddRange(
            new TabulatedResult { TenantId = TenantId, EntryId = Guid.NewGuid(), DivisionId = divisionId, RawScore = 80, FinalScore = 80 },
            new TabulatedResult { TenantId = TenantId, EntryId = Guid.NewGuid(), DivisionId = divisionId, RawScore = 95, FinalScore = 95 },
            new TabulatedResult { TenantId = TenantId, EntryId = Guid.NewGuid(), DivisionId = divisionId, RawScore = 87, FinalScore = 87 });
        await db.SaveChangesAsync();

        await service.RankDivisionAsync(divisionId);

        var ranked = await db.TabulatedResults.Where(t => t.DivisionId == divisionId).OrderBy(t => t.Rank).ToListAsync();
        ranked[0].FinalScore.Should().Be(95);
        ranked[0].Rank.Should().Be(1);
        ranked[1].FinalScore.Should().Be(87);
        ranked[1].Rank.Should().Be(2);
        ranked[2].FinalScore.Should().Be(80);
        ranked[2].Rank.Should().Be(3);
    }

    [Fact]
    public async Task ReleaseScores_Marks_All_Results_As_Released()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new ScoringService(db, tenant);

        var divisionId = Guid.NewGuid();
        db.TabulatedResults.AddRange(
            new TabulatedResult { TenantId = TenantId, EntryId = Guid.NewGuid(), DivisionId = divisionId, FinalScore = 80, Rank = 2 },
            new TabulatedResult { TenantId = TenantId, EntryId = Guid.NewGuid(), DivisionId = divisionId, FinalScore = 90, Rank = 1 });
        await db.SaveChangesAsync();

        await service.ReleaseScoresAsync(divisionId);

        var results = await db.TabulatedResults.Where(t => t.DivisionId == divisionId).ToListAsync();
        results.Should().AllSatisfy(r =>
        {
            r.IsReleased.Should().BeTrue();
            r.ReleasedAt.Should().NotBeNull();
        });
    }

    private EventEntry SetupScoringData(AppDbContext db)
    {
        var evt = new Event
        {
            TenantId = TenantId, Name = "Test Event",
            EntryDeadline = DateOnly.MaxValue, BaseEntryFee = 100
        };
        db.Events.Add(evt);

        var division = new Division
        {
            TenantId = TenantId, EventId = evt.Id, Name = "Test Div",
            Level = CheerLevel.Level2, ScoresheetType = ScoresheetType.USS
        };
        db.Divisions.Add(division);

        var template = new ScoresheetTemplate
        {
            TenantId = TenantId, DivisionId = division.Id,
            Name = "Test Sheet", Type = ScoresheetType.USS
        };
        db.ScoresheetTemplates.Add(template);

        var caption = new ScoresheetCaption
        {
            TemplateId = template.Id, Name = "Stunts",
            MaxScore = 10, Weight = 1.0m, SortOrder = 1
        };
        db.ScoresheetCaptions.Add(caption);

        db.ScoresheetSubcaptions.AddRange(
            new ScoresheetSubcaption { CaptionId = caption.Id, Name = "Difficulty", MinScore = 0, MaxScore = 10, Increment = 0.5m, Weight = 1.0m, SortOrder = 1 },
            new ScoresheetSubcaption { CaptionId = caption.Id, Name = "Execution", MinScore = 0, MaxScore = 10, Increment = 0.5m, Weight = 1.0m, SortOrder = 2 });

        var team = new Team { TenantId = TenantId, Name = "Test Team", Level = CheerLevel.Level2 };
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
        db.JudgePanelMembers.Add(new JudgePanelMember
        {
            TenantId = TenantId, PanelId = panel.Id,
            JudgeUserId = "judge-1", JudgeName = "Test Judge", Role = "Head Judge"
        });

        db.SaveChanges();
        return entry;
    }
}
