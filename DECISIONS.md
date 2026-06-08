# CheerDeck - Decisions & Assumptions

## Architecture

| Decision | Rationale |
|----------|-----------|
| **NET 10 instead of NET 9** | .NET 9 SDK was unavailable in the build environment; .NET 10 (10.0.108) was available and is the latest LTS-successor. All APIs and patterns are compatible. |
| **In-memory database by default** | Enables zero-configuration local development. SQL Server is configured via `ConnectionStrings:DefaultConnection` when ready. Toggle via `UseInMemoryDatabase` in appsettings. |
| **EF Core global query filters for tenant isolation** | Ensures cross-tenant data access is impossible by default at the ORM layer, not dependent on developers remembering WHERE clauses. Every tenant-owned entity type has a filter on `TenantId`. |
| **Soft-delete checks in service layer, not query filters** | EF Core allows only one query filter expression per entity type. Since tenant isolation uses the filter, soft-delete is handled in service layer queries (`.Where(!IsDeleted)`). This also allows admin views of deleted records. |
| **Audit trail excludes sensitive fields** | `MedicalNotes` and `EmergencyContactPhone` are excluded from audit entry serialization to comply with GDPR data minimisation. |
| **Modular monolith, not microservices** | Single solution with separate projects per concern. Club and Competition are modules sharing the domain. Simpler to develop, deploy, and debug for a startup-stage product. |
| **Blazor Server (not WASM)** | Real-time features (SignalR for running order, live scores) integrate naturally with Server mode. Components are kept clean enough to migrate to WASM or separate SPA later. |

## Domain Model

| Decision | Rationale |
|----------|-----------|
| **Tenant is either Club or EventProducer** | Two distinct tenant types with different feature access. A club can share specific data (team, athletes) with a producer for an event entry via `ClubTenantId` on `EventEntry`. |
| **EventEntry carries ClubTenantId** | This is the cross-tenant bridge: an entry references both the event producer's tenant (via the entry's own TenantId) and the originating club's tenant. The event producer can see entry data but not the club's full roster. |
| **CheerLevel as enum (Novice + Levels 1-7)** | Matches the UK cheerleading level system. Stored as integer for easy comparison and filtering. |
| **AgeGrid as reference data (not tenant-scoped)** | Age grids are NGB-standard data shared across all tenants. They have no TenantId and no query filter. |
| **Score versioning for offline-first** | Each score carries a `Version` field. When syncing offline scores, the system only applies updates where the incoming version is higher than the stored version. This prevents stale offline scores from overwriting newer online scores. |

## Integration

| Decision | Rationale |
|----------|-----------|
| **Three integration interfaces with stub implementations** | `IEligibilityProvider` (Sport:80/SportCheer), `IMusicLicenceProvider` (ClicknClear), `IPaymentGateway` (Stripe). Stubs return success for all operations to enable local development without external accounts. |
| **Swappable via `IntegrationProvider` config** | Set to `"Stub"` by default. Real implementations would be registered in `DependencyInjection.cs` based on this config value. |

## Scoring

| Decision | Rationale |
|----------|-----------|
| **USS scoresheet as primary, IASF and ICU Adaptive as enum variants** | The UK Scoring System is the most common. The scoresheet template model (Captions > Subcaptions with ranges, weights, increments) is flexible enough for all three types. |
| **Weighted scoring: Score * SubcaptionWeight * CaptionWeight** | Allows different emphasis on different skills per division/level. Weights default to 1.0 for equal weighting. |
| **Tabulation calculates RawScore - Deductions = FinalScore** | Simple model that matches how cheerleading scoring works. FinalScore is clamped to minimum 0. |

## Security & Data Protection

| Decision | Rationale |
|----------|-----------|
| **No personal data in URLs** | All entity references use GUIDs in route parameters, never names or DOBs. |
| **Medical notes and consents are never logged** | Audit trail excludes these fields. They're only displayed in athlete detail views to authorised roles. |
| **ASP.NET Core Identity with role-based access** | Eight roles covering both products. TenantId is stored as a claim for the query filter. |

## UI

| Decision | Rationale |
|----------|-----------|
| **Bootstrap 5 via Blazor template** | Already included. Functional MVP styling without additional CSS frameworks. |
| **All pages use InteractiveServer render mode** | Enables real-time interactivity and SignalR integration. |
