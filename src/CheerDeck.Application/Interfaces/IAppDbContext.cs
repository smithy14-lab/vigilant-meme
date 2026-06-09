namespace CheerDeck.Application.Interfaces;

using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Common;
using CheerDeck.Domain.Competition;
using Microsoft.EntityFrameworkCore;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }

    // Club management
    DbSet<Athlete> Athletes { get; }
    DbSet<AthleteCrossover> AthleteCrossovers { get; }
    DbSet<Guardian> Guardians { get; }
    DbSet<AthleteGuardian> AthleteGuardians { get; }
    DbSet<Coach> Coaches { get; }
    DbSet<CoachQualification> CoachQualifications { get; }
    DbSet<Venue> Venues { get; }
    DbSet<Term> Terms { get; }
    DbSet<Class> Classes { get; }
    DbSet<ClassCoach> ClassCoaches { get; }
    DbSet<Enrolment> Enrolments { get; }
    DbSet<ClassSession> ClassSessions { get; }
    DbSet<AttendanceRecord> AttendanceRecords { get; }
    DbSet<PrivateLesson> PrivateLessons { get; }
    DbSet<PrivateLessonAthlete> PrivateLessonAthletes { get; }
    DbSet<Camp> Camps { get; }
    DbSet<CampBooking> CampBookings { get; }
    DbSet<Team> Teams { get; }
    DbSet<TeamMember> TeamMembers { get; }
    DbSet<TeamMusic> TeamMusic { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLineItem> InvoiceLineItems { get; }
    DbSet<Message> Messages { get; }
    DbSet<MessageRecipient> MessageRecipients { get; }
    DbSet<Waiver> Waivers { get; }
    DbSet<WaiverSignature> WaiverSignatures { get; }
    DbSet<ChatRoom> ChatRooms { get; }
    DbSet<ChatRoomMember> ChatRoomMembers { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<AutomatedReminder> AutomatedReminders { get; }
    DbSet<ReminderLog> ReminderLogs { get; }

    // Competition
    DbSet<Event> Events { get; }
    DbSet<EventSession> EventSessions { get; }
    DbSet<SessionBlock> SessionBlocks { get; }
    DbSet<Division> Divisions { get; }
    DbSet<AgeGrid> AgeGrids { get; }
    DbSet<AgeGridDivision> AgeGridDivisions { get; }
    DbSet<EventEntry> EventEntries { get; }
    DbSet<EntryAthlete> EntryAthletes { get; }
    DbSet<RunningOrderEntry> RunningOrderEntries { get; }
    DbSet<WarmUpSlot> WarmUpSlots { get; }
    DbSet<ScoresheetTemplate> ScoresheetTemplates { get; }
    DbSet<ScoresheetCaption> ScoresheetCaptions { get; }
    DbSet<ScoresheetSubcaption> ScoresheetSubcaptions { get; }
    DbSet<JudgePanel> JudgePanels { get; }
    DbSet<JudgePanelMember> JudgePanelMembers { get; }
    DbSet<Score> Scores { get; }
    DbSet<Deduction> Deductions { get; }
    DbSet<TabulatedResult> TabulatedResults { get; }
    DbSet<ScoreCheckRequest> ScoreCheckRequests { get; }
    DbSet<Award> Awards { get; }

    // Audit
    DbSet<AuditEntry> AuditEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
