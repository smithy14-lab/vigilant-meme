# CheerDeck

A SaaS platform for the UK cheerleading market combining club management and competition systems in a single codebase.

## Prerequisites

- .NET 10 SDK (10.0.x)
- (Optional) SQL Server for persistent storage

## Quick Start

```bash
# Clone and run
cd src/CheerDeck.Web
dotnet run
```

The app starts with an in-memory database pre-seeded with demo data:

- **Stardust Cheer Academy** (demo club) - coaches, athletes, guardians, classes, teams
- **UK Cheer Championships** (demo event producer) - event, divisions, scoresheet templates, judge panel

Navigate to `https://localhost:5001` (or the port shown in console output).

## Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Club Owner | clubowner@stardust.co.uk | Club0wner! |
| Coach | coach@stardust.co.uk | C0ach1ng! |
| Guardian | parent@example.co.uk | Par3nt! |
| Event Producer | producer@ukcc.co.uk | Produc3r! |
| Judge | judge@ukcc.co.uk | Judg3! |

## Project Structure

```
CheerDeck.slnx
src/
  CheerDeck.Domain/           Pure domain model, no dependencies
    Common/                   BaseEntity, Tenant, AuditEntry
    ClubManagement/           Athletes, Coaches, Classes, Teams, Invoices, etc.
    Competition/              Events, Divisions, Scoring, Running Order, etc.
    Integration/              IEligibilityProvider, IMusicLicenceProvider, IPaymentGateway

  CheerDeck.Application/      Application services and interfaces
    Services/                 Business logic (AthleteService, ScoringService, etc.)
    Interfaces/               IAppDbContext, ITenantContext

  CheerDeck.Infrastructure/   EF Core, Identity, SignalR, Stubs
    Data/                     AppDbContext, SeedData, TenantContext
    Stubs/                    In-memory implementations of all integrations
    Hubs/                     SignalR hubs (RunningOrder, Score, Leaderboard)
    Identity/                 AppUser, AppRoles

  CheerDeck.Web/              Blazor Server UI
    Components/Pages/         All Blazor pages
    Components/Layout/        Navigation, layout

tests/
  CheerDeck.Tests/            xUnit tests
```

## Running Tests

```bash
dotnet test
```

Tests cover:
- Tenant isolation (cross-tenant data access prevention)
- Scoring tabulation (weighted scores, deductions, ranking)
- Offline score sync (version-based conflict resolution)
- Service layer (CRUD, soft-delete, enrolment, waiting lists)
- Integration stubs (eligibility, music licence, payments)

## Configuration

### Database

By default, the app uses an in-memory database. To use SQL Server:

```json
// appsettings.json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CheerDeck;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

Then run migrations:
```bash
cd src/CheerDeck.Web
dotnet ef database update
```

### Integration Providers

All three external integrations default to stub implementations:

```json
{
  "IntegrationProvider": "Stub"
}
```

Swap to real implementations by changing the provider and adding credentials:
- **Eligibility**: Sport:80 / SportCheer UK API
- **Music Licensing**: ClicknClear API
- **Payments**: Stripe API

## Reset Database

With in-memory database, simply restart the application. With SQL Server:

```bash
dotnet ef database drop --force
dotnet ef database update
```

Seed data is re-applied on startup if the database is empty.

## Architecture

- **Modular monolith**: Single solution, separate projects per concern
- **Multi-tenant**: EF Core global query filters enforce tenant isolation at the ORM layer
- **Audit trail**: All entity changes are tracked (who, what, when)
- **Soft-delete**: Personal data entities support soft-delete for GDPR compliance
- **Real-time**: SignalR hubs for running order, live scores, and leaderboards
- **Offline-first scoring**: Version-based score sync prevents data loss on flaky venue wifi

See [DECISIONS.md](DECISIONS.md) for architectural decisions and trade-offs.
See [STATUS.md](STATUS.md) for feature completion status.
