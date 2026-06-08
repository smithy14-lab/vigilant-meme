namespace CheerDeck.Tests;

using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class ServiceTests
{
    private static readonly Guid TenantId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public async Task AthleteService_Creates_And_Retrieves_Athlete()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new AthleteService(db, tenant);

        var athlete = await service.CreateAsync(new Athlete
        {
            FirstName = "Test", LastName = "Athlete",
            DateOfBirth = new DateOnly(2012, 6, 15),
            Level = CheerLevel.Level2
        });

        athlete.Id.Should().NotBeEmpty();
        athlete.TenantId.Should().Be(TenantId);

        var retrieved = await service.GetByIdAsync(athlete.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FullName.Should().Be("Test Athlete");
    }

    [Fact]
    public async Task AthleteService_SoftDelete_Hides_Athlete()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var service = new AthleteService(db, tenant);

        var athlete = await service.CreateAsync(new Athlete
        {
            FirstName = "ToDelete", LastName = "Athlete",
            DateOfBirth = new DateOnly(2013, 1, 1),
            Level = CheerLevel.Novice
        });

        await service.SoftDeleteAsync(athlete.Id);

        var all = await service.GetAllAsync();
        all.Should().BeEmpty();

        // But still in DB
        var raw = await db.Athletes.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == athlete.Id);
        raw.Should().NotBeNull();
        raw!.IsDeleted.Should().BeTrue();
        raw.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ClassService_Enrols_And_Detects_Full_Class()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var classService = new ClassService(db, tenant);

        var cls = await classService.CreateAsync(new Class
        {
            Name = "Small Class",
            DayOfWeek = DayOfWeekEnum.Monday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0),
            Capacity = 2,
            PricePerSession = 10m
        });

        var a1 = new Athlete { TenantId = TenantId, FirstName = "A1", LastName = "T", DateOfBirth = new DateOnly(2012, 1, 1), Level = CheerLevel.Novice };
        var a2 = new Athlete { TenantId = TenantId, FirstName = "A2", LastName = "T", DateOfBirth = new DateOnly(2012, 1, 1), Level = CheerLevel.Novice };
        var a3 = new Athlete { TenantId = TenantId, FirstName = "A3", LastName = "T", DateOfBirth = new DateOnly(2012, 1, 1), Level = CheerLevel.Novice };
        db.Athletes.AddRange(a1, a2, a3);
        await db.SaveChangesAsync();

        var e1 = await classService.EnrolAthleteAsync(cls.Id, a1.Id);
        var e2 = await classService.EnrolAthleteAsync(cls.Id, a2.Id);
        var e3 = await classService.EnrolAthleteAsync(cls.Id, a3.Id);

        e1.Status.Should().Be(EnrolmentStatus.Active);
        e2.Status.Should().Be(EnrolmentStatus.Active);
        e3.Status.Should().Be(EnrolmentStatus.WaitingList);
        e3.WaitingListPosition.Should().Be(1);
    }

    [Fact]
    public async Task CampService_Books_And_WaitingList()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var campService = new CampService(db, tenant);

        var camp = await campService.CreateAsync(new Camp
        {
            Name = "Small Camp", StartDate = new DateOnly(2026, 7, 1),
            EndDate = new DateOnly(2026, 7, 2), Capacity = 1, Price = 50m
        });

        var a1 = new Athlete { TenantId = TenantId, FirstName = "A1", LastName = "C", DateOfBirth = new DateOnly(2012, 1, 1), Level = CheerLevel.Novice };
        var a2 = new Athlete { TenantId = TenantId, FirstName = "A2", LastName = "C", DateOfBirth = new DateOnly(2012, 1, 1), Level = CheerLevel.Novice };
        db.Athletes.AddRange(a1, a2);
        await db.SaveChangesAsync();

        var b1 = await campService.BookAthleteAsync(camp.Id, a1.Id);
        var b2 = await campService.BookAthleteAsync(camp.Id, a2.Id);

        b1.Status.Should().Be(CampBookingStatus.Confirmed);
        b2.Status.Should().Be(CampBookingStatus.WaitingList);
    }

    [Fact]
    public async Task TermService_Duplicates_Term_With_Classes()
    {
        var (db, tenant) = TestDbContextFactory.CreateWithTenant(TenantId);
        var termService = new TermService(db, tenant);

        var term = await termService.CreateAsync(new Term
        {
            Name = "Autumn 2025",
            StartDate = new DateOnly(2025, 9, 1),
            EndDate = new DateOnly(2025, 12, 20)
        });

        var cls = new Class
        {
            TenantId = TenantId, Name = "Test Class", TermId = term.Id,
            DayOfWeek = DayOfWeekEnum.Monday, StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0), Capacity = 20, PricePerSession = 10m
        };
        db.Classes.Add(cls);
        await db.SaveChangesAsync();

        var newTerm = await termService.DuplicateAsync(
            term.Id, "Spring 2026",
            new DateOnly(2026, 1, 5),
            new DateOnly(2026, 3, 27));

        newTerm.Name.Should().Be("Spring 2026");
        var newClasses = await db.Classes.Where(c => c.TermId == newTerm.Id).ToListAsync();
        newClasses.Should().HaveCount(1);
        newClasses[0].Name.Should().Be("Test Class");
    }

    [Fact]
    public void Athlete_GetAgeOnDate_Calculates_Correctly()
    {
        var athlete = new Athlete
        {
            FirstName = "Test", LastName = "Age",
            DateOfBirth = new DateOnly(2012, 6, 15),
            Level = CheerLevel.Level2
        };

        athlete.GetAgeOnDate(new DateOnly(2025, 6, 14)).Should().Be(12);
        athlete.GetAgeOnDate(new DateOnly(2025, 6, 15)).Should().Be(13);
        athlete.GetAgeOnDate(new DateOnly(2025, 12, 31)).Should().Be(13);
    }
}
