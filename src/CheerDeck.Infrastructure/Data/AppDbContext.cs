namespace CheerDeck.Infrastructure.Data;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Common;
using CheerDeck.Domain.Competition;
using CheerDeck.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

public class AppDbContext : IdentityDbContext<AppUser>, IAppDbContext
{
    private readonly ITenantContext _tenantContext;
    private Guid _tenantId;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
        _tenantId = tenantContext.TenantId;
    }
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Athlete> Athletes => Set<Athlete>();
    public DbSet<AthleteCrossover> AthleteCrossovers => Set<AthleteCrossover>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<AthleteGuardian> AthleteGuardians => Set<AthleteGuardian>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<CoachQualification> CoachQualifications => Set<CoachQualification>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassCoach> ClassCoaches => Set<ClassCoach>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<ClassSession> ClassSessions => Set<ClassSession>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<PrivateLesson> PrivateLessons => Set<PrivateLesson>();
    public DbSet<PrivateLessonAthlete> PrivateLessonAthletes => Set<PrivateLessonAthlete>();
    public DbSet<Camp> Camps => Set<Camp>();
    public DbSet<CampBooking> CampBookings => Set<CampBooking>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamMusic> TeamMusic => Set<TeamMusic>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>();
    public DbSet<Waiver> Waivers => Set<Waiver>();
    public DbSet<WaiverSignature> WaiverSignatures => Set<WaiverSignature>();
    public DbSet<ChatRoom> ChatRooms => Set<ChatRoom>();
    public DbSet<ChatRoomMember> ChatRoomMembers => Set<ChatRoomMember>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<AutomatedReminder> AutomatedReminders => Set<AutomatedReminder>();
    public DbSet<ReminderLog> ReminderLogs => Set<ReminderLog>();

    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<SessionBlock> SessionBlocks => Set<SessionBlock>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<AgeGrid> AgeGrids => Set<AgeGrid>();
    public DbSet<AgeGridDivision> AgeGridDivisions => Set<AgeGridDivision>();
    public DbSet<EventEntry> EventEntries => Set<EventEntry>();
    public DbSet<EntryAthlete> EntryAthletes => Set<EntryAthlete>();
    public DbSet<RunningOrderEntry> RunningOrderEntries => Set<RunningOrderEntry>();
    public DbSet<WarmUpSlot> WarmUpSlots => Set<WarmUpSlot>();
    public DbSet<ScoresheetTemplate> ScoresheetTemplates => Set<ScoresheetTemplate>();
    public DbSet<ScoresheetCaption> ScoresheetCaptions => Set<ScoresheetCaption>();
    public DbSet<ScoresheetSubcaption> ScoresheetSubcaptions => Set<ScoresheetSubcaption>();
    public DbSet<JudgePanel> JudgePanels => Set<JudgePanel>();
    public DbSet<JudgePanelMember> JudgePanelMembers => Set<JudgePanelMember>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Deduction> Deductions => Set<Deduction>();
    public DbSet<TabulatedResult> TabulatedResults => Set<TabulatedResult>();
    public DbSet<ScoreCheckRequest> ScoreCheckRequests => Set<ScoreCheckRequest>();
    public DbSet<Award> Awards => Set<Award>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ApplyTenantFilters(builder);
        ApplySoftDeleteFilters(builder);
        ConfigureRelationships(builder);
        ConfigureIndexes(builder);
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        // Reference _tenantId field so EF Core evaluates per-query, not at model creation
        builder.Entity<Athlete>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<AthleteCrossover>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Guardian>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<AthleteGuardian>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Coach>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<CoachQualification>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Venue>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Term>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Class>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ClassCoach>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Enrolment>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ClassSession>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<AttendanceRecord>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<PrivateLesson>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<PrivateLessonAthlete>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Camp>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<CampBooking>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Team>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<TeamMember>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<TeamMusic>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Invoice>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<InvoiceLineItem>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Message>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<MessageRecipient>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Waiver>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<WaiverSignature>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ChatRoom>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ChatRoomMember>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ChatMessage>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<AutomatedReminder>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ReminderLog>().HasQueryFilter(e => e.TenantId == _tenantId);

        builder.Entity<Event>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<EventSession>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<SessionBlock>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Division>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<EventEntry>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<EntryAthlete>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<RunningOrderEntry>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<WarmUpSlot>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ScoresheetTemplate>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<JudgePanel>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<JudgePanelMember>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Score>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Deduction>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<TabulatedResult>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<ScoreCheckRequest>().HasQueryFilter(e => e.TenantId == _tenantId);
        builder.Entity<Award>().HasQueryFilter(e => e.TenantId == _tenantId);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder builder)
    {
        // Soft-delete is combined with tenant filter using HasQueryFilter override
        // Since EF Core merges filters, the tenant filter above already applies
        // Soft-delete checks are done in service layer queries (.Where(!IsDeleted))
        // to allow admins to view deleted records when needed
    }

    private static void ConfigureRelationships(ModelBuilder builder)
    {
        builder.Entity<AthleteGuardian>()
            .HasOne(ag => ag.Athlete).WithMany(a => a.Guardians)
            .HasForeignKey(ag => ag.AthleteId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<AthleteGuardian>()
            .HasOne(ag => ag.Guardian).WithMany(g => g.Athletes)
            .HasForeignKey(ag => ag.GuardianId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ClassCoach>()
            .HasOne(cc => cc.Class).WithMany(c => c.Coaches)
            .HasForeignKey(cc => cc.ClassId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<ClassCoach>()
            .HasOne(cc => cc.Coach).WithMany(c => c.ClassAssignments)
            .HasForeignKey(cc => cc.CoachId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TeamMember>()
            .HasOne(tm => tm.Team).WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<TeamMember>()
            .HasOne(tm => tm.Athlete).WithMany(a => a.TeamMemberships)
            .HasForeignKey(tm => tm.AthleteId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WarmUpSlot>()
            .HasOne(w => w.RunningOrderEntry).WithOne(r => r.WarmUpSlot)
            .HasForeignKey<WarmUpSlot>(w => w.RunningOrderEntryId);

        builder.Entity<EventEntry>()
            .HasOne(e => e.Team).WithMany()
            .HasForeignKey(e => e.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<EventEntry>()
            .HasOne(e => e.MusicFile).WithMany()
            .HasForeignKey(e => e.MusicFileId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Invoice>().Property(i => i.Total).HasColumnType("decimal(18,2)");
        builder.Entity<Invoice>().Property(i => i.Subtotal).HasColumnType("decimal(18,2)");
        builder.Entity<Invoice>().Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Entity<InvoiceLineItem>().Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Entity<InvoiceLineItem>().Property(i => i.Total).HasColumnType("decimal(18,2)");
        builder.Entity<Class>().Property(c => c.PricePerSession).HasColumnType("decimal(18,2)");
        builder.Entity<Class>().Property(c => c.TermPrice).HasColumnType("decimal(18,2)");
        builder.Entity<Camp>().Property(c => c.Price).HasColumnType("decimal(18,2)");
        builder.Entity<PrivateLesson>().Property(p => p.Price).HasColumnType("decimal(18,2)");
        builder.Entity<EventEntry>().Property(e => e.EntryFee).HasColumnType("decimal(18,2)");
        builder.Entity<Event>().Property(e => e.BaseEntryFee).HasColumnType("decimal(18,2)");
        builder.Entity<Division>().Property(d => d.EntryFeeOverride).HasColumnType("decimal(18,2)");
        builder.Entity<Score>().Property(s => s.Value).HasColumnType("decimal(18,4)");
        builder.Entity<Deduction>().Property(d => d.Points).HasColumnType("decimal(18,4)");
        builder.Entity<TabulatedResult>().Property(t => t.RawScore).HasColumnType("decimal(18,4)");
        builder.Entity<TabulatedResult>().Property(t => t.DeductionTotal).HasColumnType("decimal(18,4)");
        builder.Entity<TabulatedResult>().Property(t => t.FinalScore).HasColumnType("decimal(18,4)");
        builder.Entity<ScoresheetCaption>().Property(c => c.MaxScore).HasColumnType("decimal(18,4)");
        builder.Entity<ScoresheetCaption>().Property(c => c.Weight).HasColumnType("decimal(18,4)");
        builder.Entity<ScoresheetSubcaption>().Property(s => s.MinScore).HasColumnType("decimal(18,4)");
        builder.Entity<ScoresheetSubcaption>().Property(s => s.MaxScore).HasColumnType("decimal(18,4)");
        builder.Entity<ScoresheetSubcaption>().Property(s => s.Increment).HasColumnType("decimal(18,4)");
        builder.Entity<ScoresheetSubcaption>().Property(s => s.Weight).HasColumnType("decimal(18,4)");
    }

    private static void ConfigureIndexes(ModelBuilder builder)
    {
        builder.Entity<Athlete>().HasIndex(a => a.TenantId);
        builder.Entity<Coach>().HasIndex(c => c.TenantId);
        builder.Entity<Guardian>().HasIndex(g => g.TenantId);
        builder.Entity<Class>().HasIndex(c => c.TenantId);
        builder.Entity<Event>().HasIndex(e => e.TenantId);
        builder.Entity<EventEntry>().HasIndex(e => new { e.EventId, e.DivisionId });
        builder.Entity<Score>().HasIndex(s => new { s.EntryId, s.PanelMemberId, s.SubcaptionId }).IsUnique();
        builder.Entity<TabulatedResult>().HasIndex(t => new { t.EntryId, t.DivisionId }).IsUnique();
        builder.Entity<Tenant>().HasIndex(t => t.Slug).IsUnique();
    }

    public override int SaveChanges()
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = base.SaveChanges();
        if (auditEntries.Count > 0)
        {
            AuditEntries.AddRange(auditEntries);
            base.SaveChanges();
        }
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var auditEntries = OnBeforeSaveChanges();
        var result = await base.SaveChangesAsync(ct);
        if (auditEntries.Count > 0)
        {
            AuditEntries.AddRange(auditEntries);
            await base.SaveChangesAsync(ct);
        }
        return result;
    }

    private List<AuditEntry> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        var entries = new List<AuditEntry>();
        var excludedProperties = new HashSet<string> { "MedicalNotes", "EmergencyContactPhone" };

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditEntry || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            if (entry.Entity is not BaseEntity baseEntity)
                continue;

            var auditEntry = new AuditEntry
            {
                EntityType = entry.Entity.GetType().Name,
                EntityId = baseEntity.Id,
                Action = entry.State.ToString(),
                UserId = _tenantContext.UserId,
                TenantId = (entry.Entity as TenantEntity)?.TenantId
            };

            if (entry.State == EntityState.Modified)
            {
                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties.Where(p => p.IsModified && !excludedProperties.Contains(p.Metadata.Name)))
                {
                    oldValues[prop.Metadata.Name] = prop.OriginalValue;
                    newValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                auditEntry.OldValues = JsonSerializer.Serialize(oldValues);
                auditEntry.NewValues = JsonSerializer.Serialize(newValues);
            }

            entries.Add(auditEntry);
        }

        return entries;
    }
}
