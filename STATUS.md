# CheerDeck - Status

## Done

### Foundation
- [x] Solution structure (Domain, Application, Infrastructure, Web, Tests)
- [x] Multi-tenancy with EF Core global query filters
- [x] Audit trail on all entity changes (excludes sensitive fields)
- [x] Soft-delete on personal-data entities
- [x] ASP.NET Core Identity with 8 roles
- [x] Seed data (demo club + demo event producer)
- [x] In-memory database for zero-config local dev
- [x] SignalR hubs (RunningOrder, Score, Leaderboard)

### Club Management
- [x] Athlete CRUD with DOB, level, crossovers, medical notes, consents
- [x] Guardian CRUD with athlete linking
- [x] Coach CRUD with qualifications and credential expiry tracking
- [x] Venues CRUD
- [x] Terms (create, duplicate with classes)
- [x] Classes with capacity, coaches, enrolments
- [x] Enrolment with waiting list (auto-waitlist when full)
- [x] Class sessions and attendance register
- [x] Private lessons (1:1 and small group)
- [x] Camps and clinics with waiting lists and auto-invite
- [x] Teams with members and music
- [x] Invoicing with line items
- [x] Messaging (club-wide, per-class, individual)
- [x] Blazor UI pages for all of the above

### Competition System
- [x] Event setup with sessions, blocks, divisions
- [x] SportCheer UK age grid as reference data
- [x] USS scoresheet templates (captions, subcaptions, weights, ranges)
- [x] One-button entry from club roster with eligibility validation
- [x] Music file association with licence verification
- [x] Running order with status tracking (Scheduled -> InWarmUp -> OnDeck -> OnFloor -> Performing -> Completed)
- [x] Warm-up slot scheduling
- [x] Judge scoring with version-based offline sync
- [x] Tabulation (weighted scores - deductions = final score)
- [x] Ranking and controlled score release
- [x] Score check request workflow
- [x] Awards
- [x] Blazor UI pages for events, running order, scoring, results, leaderboard, entry

### Integration
- [x] IEligibilityProvider interface + stub (Sport:80/SportCheer)
- [x] IMusicLicenceProvider interface + stub (ClicknClear)
- [x] IPaymentGateway interface + stub (Stripe)
- [x] All three swappable via configuration

### Tests
- [x] Tenant isolation tests (athletes, classes, events, coaches cross-tenant)
- [x] Scoring tabulation tests (weighted scores, deductions, ranking, release)
- [x] Offline sync tests (version-based conflict resolution)
- [x] Service tests (CRUD, soft-delete, enrolment/waiting list, term duplication)
- [x] Integration stub tests (eligibility, music licence, payments)

## Partial / Thin Version

| Feature | What's done | What a fuller version would add |
|---------|-------------|--------------------------------|
| Authentication | Identity configured, seed users with roles | Login/logout UI, role-based page access, claims-based TenantId from login |
| Music playback | File metadata stored, music attached to entries | Actual file upload, audio player component, cue/play performance-day view |
| Real-time running order | SignalR hub + status updates in UI | Push notifications to coach phones, auto-scroll on displays |
| Offline scoring | Version-based sync logic, offline flag | Service Worker / PWA manifest, IndexedDB local storage, background sync |
| Live leaderboard | Results page with large-text display mode | Auto-refresh via SignalR, dedicated display mode for venue screens |
| Email notifications | Message model supports SendEmail flag | Actual email sending (SendGrid/SES integration) |
| Reporting | Basic data available via queries | Dashboard charts (revenue, attendance trends, retention) |
| Coach invoicing | Invoice model supports CoachId | Separate coach payment workflow, bank details |
| GDPR requests | Soft-delete and audit trail in place | Data export endpoint, automated erasure workflow |
| Announcer/MC view | Running order data available | Dedicated view with team info, routine music cue |

## Not Started
- [ ] SQL Server migrations (using InMemory; migration generation ready when SQL Server is configured)
- [ ] File upload storage (S3/Azure Blob)
- [ ] Email sending integration
- [ ] PWA / Service Worker for offline capability
- [ ] Dashboard charts/visualisations
- [ ] IASF and ICU Adaptive scoresheet templates (model supports them, templates not seeded)
- [ ] Cross-club crossover validation
- [ ] Stripe webhook handling for payment confirmations
- [ ] Rate limiting and request throttling
- [ ] Automated regression test suite for UI

## Known Issues
- InMemory database doesn't enforce unique constraints the same way SQL Server does
- TenantId must be set manually in the current HTTP context (no login flow yet)
- The audit trail writes a second SaveChanges call which isn't ideal for performance in high-throughput scenarios
