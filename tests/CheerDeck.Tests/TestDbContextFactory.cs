namespace CheerDeck.Tests;

using CheerDeck.Application.Interfaces;
using CheerDeck.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public static class TestDbContextFactory
{
    public static AppDbContext Create(Guid tenantId)
    {
        var tenantContext = new FixedTenantContext(tenantId, "test-user", "ClubOwner");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options, tenantContext);
        db.Database.EnsureCreated();
        return db;
    }

    public static (AppDbContext Db, ITenantContext Tenant) CreateWithTenant(Guid tenantId)
    {
        var tenantContext = new FixedTenantContext(tenantId, "test-user", "ClubOwner");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options, tenantContext);
        db.Database.EnsureCreated();
        return (db, tenantContext);
    }
}
