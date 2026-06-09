# CheerDeck

Two connected products for the UK cheerleading market, sharing one backend and database.

## Products

### CheerDeck Club (port 5200)
Club management app for coaches and parents. Manage athletes, classes, bookings, attendance, invoicing, messaging.

### CheerDeck Competitions (port 5300)
Competition management app for event producers, judges, and coaches. Event setup, entries, running order, live scoring, results, leaderboards.

### CheerDeck API (port 5100)
Shared REST API backend. Powers both web apps and future mobile apps (Android/iOS via MAUI Blazor Hybrid).

## Prerequisites

- .NET 9 SDK (9.0.x)
- (Optional) SQL Server for persistent storage

## Quick Start

Run either app independently — each has its own seed data:

```bash
# Club app
cd src/CheerDeck.Club.Web
dotnet run
# Open http://localhost:5200

# Competition app (separate terminal)
cd src/CheerDeck.Competition.Web
dotnet run
# Open http://localhost:5300

# API only
cd src/CheerDeck.Api
dotnet run
# http://localhost:5100/api/...
```

## Demo Accounts

| Role | Email | Password | App |
|------|-------|----------|-----|
| Club Owner | clubowner@stardust.co.uk | Club0wner! | Club |
| Coach | coach@stardust.co.uk | C0ach1ng! | Club |
| Guardian | parent@example.co.uk | Par3nt! | Club |
| Event Producer | producer@ukcc.co.uk | Produc3r! | Competition |
| Judge | judge@ukcc.co.uk | Judg3! | Competition |

## Project Structure

```
CheerDeck.slnx
src/
  CheerDeck.Domain/              Pure domain model, no dependencies
  CheerDeck.Application/         Application services and interfaces
  CheerDeck.Infrastructure/      EF Core, Identity, SignalR, Stubs

  CheerDeck.Api/                 Shared REST API backend
    Controllers/                 15 API controllers

  CheerDeck.Shared.UI/           Shared Blazor components library
    Components/                  StatusBadge, LoadingSpinner, EmptyState

  CheerDeck.Club.Web/            Club management Blazor Server app
    Components/Pages/            Athletes, Classes, Coaches, Teams, etc.

  CheerDeck.Competition.Web/     Competition Blazor Server app
    Components/Pages/            Events, Scoring, Results, Leaderboard, etc.

  CheerDeck.Web/                 (Legacy combined app - superseded)

tests/
  CheerDeck.Tests/               xUnit tests
```

## Running Tests

```bash
dotnet test
```

## Configuration

Each app has its own `appsettings.json`. By default all use in-memory database.

For SQL Server, update `appsettings.json`:
```json
{
  "UseInMemoryDatabase": false,
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CheerDeck;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

## Future: Mobile Apps

The architecture supports MAUI Blazor Hybrid for native Android/iOS:
```
src/
  CheerDeck.Club.Mobile/         Club MAUI app (shares Club.Web components)
  CheerDeck.Competition.Mobile/  Competition MAUI app (shares Competition.Web components)
```

Both mobile apps would consume the API and reuse Blazor components from Shared.UI.

See [DECISIONS.md](DECISIONS.md) and [STATUS.md](STATUS.md) for details.
