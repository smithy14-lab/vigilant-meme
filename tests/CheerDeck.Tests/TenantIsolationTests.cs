namespace CheerDeck.Tests;

using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Common;
using CheerDeck.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class TenantIsolationTests
{
    private static readonly Guid Tenant1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Tenant2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Athletes_Are_Isolated_Between_Tenants()
    {
        var dbName = Guid.NewGuid().ToString();

        // Seed data as Tenant1
        var tenant1Context = new FixedTenantContext(Tenant1);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;

        using (var db = new AppDbContext(options, tenant1Context))
        {
            db.Database.EnsureCreated();
            db.Athletes.Add(new Athlete
            {
                TenantId = Tenant1, FirstName = "Alice", LastName = "T1",
                DateOfBirth = new DateOnly(2012, 1, 1), Level = CheerLevel.Level2
            });
            db.Athletes.Add(new Athlete
            {
                TenantId = Tenant2, FirstName = "Bob", LastName = "T2",
                DateOfBirth = new DateOnly(2013, 1, 1), Level = CheerLevel.Level3
            });
            await db.SaveChangesAsync();
        }

        // Query as Tenant1 - should only see Alice
        using (var db = new AppDbContext(options, tenant1Context))
        {
            var athletes = await db.Athletes.ToListAsync();
            athletes.Should().HaveCount(1);
            athletes[0].FirstName.Should().Be("Alice");
        }

        // Query as Tenant2 - should only see Bob
        var tenant2Context = new FixedTenantContext(Tenant2);
        using (var db = new AppDbContext(options, tenant2Context))
        {
            var athletes = await db.Athletes.ToListAsync();
            athletes.Should().HaveCount(1);
            athletes[0].FirstName.Should().Be("Bob");
        }
    }

    [Fact]
    public async Task Classes_Are_Isolated_Between_Tenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;

        using (var db = new AppDbContext(options, new FixedTenantContext(Tenant1)))
        {
            db.Database.EnsureCreated();
            db.Classes.Add(new Class
            {
                TenantId = Tenant1, Name = "T1 Class",
                DayOfWeek = DayOfWeekEnum.Monday, StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0), Capacity = 20, PricePerSession = 10m
            });
            db.Classes.Add(new Class
            {
                TenantId = Tenant2, Name = "T2 Class",
                DayOfWeek = DayOfWeekEnum.Tuesday, StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0), Capacity = 15, PricePerSession = 12m
            });
            await db.SaveChangesAsync();
        }

        using (var db = new AppDbContext(options, new FixedTenantContext(Tenant1)))
        {
            var classes = await db.Classes.ToListAsync();
            classes.Should().HaveCount(1);
            classes[0].Name.Should().Be("T1 Class");
        }
    }

    [Fact]
    public async Task Events_Are_Isolated_Between_Tenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;

        using (var db = new AppDbContext(options, new FixedTenantContext(Tenant1)))
        {
            db.Database.EnsureCreated();
            db.Events.Add(new Domain.Competition.Event
            {
                TenantId = Tenant1, Name = "T1 Event", EntryDeadline = DateOnly.MaxValue, BaseEntryFee = 100
            });
            db.Events.Add(new Domain.Competition.Event
            {
                TenantId = Tenant2, Name = "T2 Event", EntryDeadline = DateOnly.MaxValue, BaseEntryFee = 200
            });
            await db.SaveChangesAsync();
        }

        using (var db = new AppDbContext(options, new FixedTenantContext(Tenant2)))
        {
            var events = await db.Events.ToListAsync();
            events.Should().HaveCount(1);
            events[0].Name.Should().Be("T2 Event");
        }
    }

    [Fact]
    public async Task Coaches_Are_Isolated_Between_Tenants()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName).Options;

        using (var db = new AppDbContext(options, new FixedTenantContext(Tenant1)))
        {
            db.Database.EnsureCreated();
            db.Coaches.Add(new Coach { TenantId = Tenant1, FirstName = "Coach1", LastName = "T1", Email = "c1@t1.com" });
            db.Coaches.Add(new Coach { TenantId = Tenant2, FirstName = "Coach2", LastName = "T2", Email = "c2@t2.com" });
            await db.SaveChangesAsync();
        }

        using (var db = new AppDbContext(options, new FixedTenantContext(Tenant1)))
        {
            var coaches = await db.Coaches.ToListAsync();
            coaches.Should().HaveCount(1);
            coaches[0].FirstName.Should().Be("Coach1");
        }
    }
}
