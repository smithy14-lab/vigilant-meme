namespace CheerDeck.Infrastructure.Data;

using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Common;
using CheerDeck.Domain.Competition;
using CheerDeck.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class SeedData
{
    public static readonly Guid ClubTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ProducerTenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task InitializeAsync(IServiceProvider services, bool seedDemoData = true)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var useInMemory = config.GetValue<bool>("UseInMemoryDatabase") ||
                          string.IsNullOrEmpty(config.GetConnectionString("DefaultConnection"));

        if (useInMemory)
            await db.Database.EnsureCreatedAsync();
        else
            await db.Database.EnsureCreatedAsync();

        await SeedRolesAsync(scope.ServiceProvider);

        if (!await db.Tenants.AnyAsync())
        {
            await SeedTenantsAsync(db);
            await SeedUsersAsync(scope.ServiceProvider, db);
        }

        if (seedDemoData && !await db.Athletes.IgnoreQueryFilters().AnyAsync())
        {
            await SeedClubDataAsync(db);
            await SeedCompetitionDataAsync(db);
        }
    }

    private static async Task SeedRolesAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedTenantsAsync(AppDbContext db)
    {
        db.Tenants.AddRange(
            new Tenant
            {
                Id = ClubTenantId,
                Name = "Stardust Cheer Academy",
                Slug = "stardust",
                Type = TenantType.Club,
                ContactEmail = "info@stardustcheer.co.uk",
                ContactPhone = "07700 900123",
                Address = "Unit 5, Riverside Park, Manchester M1 2AB"
            },
            new Tenant
            {
                Id = ProducerTenantId,
                Name = "UK Cheer Championships",
                Slug = "ukcc",
                Type = TenantType.EventProducer,
                ContactEmail = "events@ukcheerchamps.co.uk",
                ContactPhone = "07700 900456",
                Address = "NEC, Birmingham B40 1NT"
            });
        await db.SaveChangesAsync();
    }

    private static async Task SeedUsersAsync(IServiceProvider sp, AppDbContext db)
    {
        var userManager = sp.GetRequiredService<UserManager<AppUser>>();

        var users = new (AppUser User, string Password, string Role, Guid TenantId)[]
        {
            (new AppUser { UserName = "clubowner@stardust.co.uk", Email = "clubowner@stardust.co.uk", FullName = "Sarah Mitchell", TenantId = ClubTenantId }, "Club0wner!", AppRoles.ClubOwner, ClubTenantId),
            (new AppUser { UserName = "coach@stardust.co.uk", Email = "coach@stardust.co.uk", FullName = "James Taylor", TenantId = ClubTenantId }, "C0ach1ng!", AppRoles.Coach, ClubTenantId),
            (new AppUser { UserName = "parent@example.co.uk", Email = "parent@example.co.uk", FullName = "Emma Wilson", TenantId = ClubTenantId }, "Par3nt!", AppRoles.Guardian, ClubTenantId),
            (new AppUser { UserName = "producer@ukcc.co.uk", Email = "producer@ukcc.co.uk", FullName = "Mark Thompson", TenantId = ProducerTenantId }, "Produc3r!", AppRoles.EventProducer, ProducerTenantId),
            (new AppUser { UserName = "judge@ukcc.co.uk", Email = "judge@ukcc.co.uk", FullName = "Helen Clarke", TenantId = ProducerTenantId }, "Judg3!", AppRoles.Judge, ProducerTenantId),
        };

        foreach (var (user, password, role, _) in users)
        {
            if (await userManager.FindByEmailAsync(user.Email!) == null)
            {
                await userManager.CreateAsync(user, password);
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    private static async Task SeedClubDataAsync(AppDbContext db)
    {
        // Temporarily bypass the tenant filter by using the base context
        var coach1Id = Guid.NewGuid();
        var coach2Id = Guid.NewGuid();

        var coaches = new[]
        {
            new Coach
            {
                Id = coach1Id, TenantId = ClubTenantId,
                FirstName = "James", LastName = "Taylor",
                Email = "coach@stardust.co.uk", Phone = "07700 900201",
                Qualifications = new List<CoachQualification>
                {
                    new() { TenantId = ClubTenantId, QualificationType = "UKCC Level 2", CertificateNumber = "UKCC-2-12345", IssuedDate = new DateOnly(2023, 3, 15), ExpiryDate = new DateOnly(2026, 3, 15), IsVerified = true },
                    new() { TenantId = ClubTenantId, QualificationType = "DBS Enhanced", CertificateNumber = "DBS-98765", IssuedDate = new DateOnly(2023, 1, 10), ExpiryDate = new DateOnly(2026, 1, 10), IsVerified = true },
                    new() { TenantId = ClubTenantId, QualificationType = "First Aid", CertificateNumber = "FA-54321", IssuedDate = new DateOnly(2024, 6, 1), ExpiryDate = new DateOnly(2027, 6, 1), IsVerified = true },
                }
            },
            new Coach
            {
                Id = coach2Id, TenantId = ClubTenantId,
                FirstName = "Amy", LastName = "Roberts",
                Email = "amy@stardust.co.uk", Phone = "07700 900202",
                Qualifications = new List<CoachQualification>
                {
                    new() { TenantId = ClubTenantId, QualificationType = "UKCC Level 1", CertificateNumber = "UKCC-1-67890", IssuedDate = new DateOnly(2024, 1, 20), ExpiryDate = new DateOnly(2027, 1, 20), IsVerified = true },
                    new() { TenantId = ClubTenantId, QualificationType = "DBS Enhanced", CertificateNumber = "DBS-11111", IssuedDate = new DateOnly(2024, 2, 1), ExpiryDate = new DateOnly(2027, 2, 1), IsVerified = true },
                }
            }
        };
        db.Coaches.AddRange(coaches);

        var venue = new Venue
        {
            TenantId = ClubTenantId,
            Name = "Riverside Sports Hall",
            Address = "Riverside Park, Manchester M1 2AB",
            Postcode = "M1 2AB",
            DefaultCapacity = 30
        };
        db.Venues.Add(venue);

        var term = new Term
        {
            TenantId = ClubTenantId,
            Name = "Autumn 2025",
            StartDate = new DateOnly(2025, 9, 1),
            EndDate = new DateOnly(2025, 12, 20),
            IsActive = true
        };
        db.Terms.Add(term);

        var classes = new[]
        {
            new Class { TenantId = ClubTenantId, Name = "Tiny Stars (Age 4-6)", TermId = term.Id, VenueId = venue.Id, DayOfWeek = DayOfWeekEnum.Monday, StartTime = new TimeOnly(16, 0), EndTime = new TimeOnly(17, 0), Capacity = 20, Level = CheerLevel.Novice, PricePerSession = 8.50m, TermPrice = 120m },
            new Class { TenantId = ClubTenantId, Name = "Junior All Stars", TermId = term.Id, VenueId = venue.Id, DayOfWeek = DayOfWeekEnum.Tuesday, StartTime = new TimeOnly(17, 0), EndTime = new TimeOnly(18, 30), Capacity = 25, Level = CheerLevel.Level2, PricePerSession = 10m, TermPrice = 140m },
            new Class { TenantId = ClubTenantId, Name = "Senior Elite", TermId = term.Id, VenueId = venue.Id, DayOfWeek = DayOfWeekEnum.Wednesday, StartTime = new TimeOnly(18, 0), EndTime = new TimeOnly(20, 0), Capacity = 24, Level = CheerLevel.Level4, PricePerSession = 12m, TermPrice = 168m },
            new Class { TenantId = ClubTenantId, Name = "Open Tumbling", TermId = term.Id, VenueId = venue.Id, DayOfWeek = DayOfWeekEnum.Thursday, StartTime = new TimeOnly(17, 30), EndTime = new TimeOnly(18, 30), Capacity = 15, Level = null, PricePerSession = 9m, TermPrice = 126m },
        };
        db.Classes.AddRange(classes);

        db.ClassCoaches.AddRange(
            new ClassCoach { TenantId = ClubTenantId, ClassId = classes[0].Id, CoachId = coach2Id, IsLead = true },
            new ClassCoach { TenantId = ClubTenantId, ClassId = classes[1].Id, CoachId = coach1Id, IsLead = true },
            new ClassCoach { TenantId = ClubTenantId, ClassId = classes[1].Id, CoachId = coach2Id },
            new ClassCoach { TenantId = ClubTenantId, ClassId = classes[2].Id, CoachId = coach1Id, IsLead = true },
            new ClassCoach { TenantId = ClubTenantId, ClassId = classes[3].Id, CoachId = coach1Id, IsLead = true });

        var guardian1 = new Guardian { TenantId = ClubTenantId, FirstName = "Emma", LastName = "Wilson", Email = "parent@example.co.uk", Phone = "07700 900301" };
        var guardian2 = new Guardian { TenantId = ClubTenantId, FirstName = "David", LastName = "Brown", Email = "david.brown@example.co.uk", Phone = "07700 900302" };
        var guardian3 = new Guardian { TenantId = ClubTenantId, FirstName = "Lisa", LastName = "Jones", Email = "lisa.jones@example.co.uk", Phone = "07700 900303" };
        db.Guardians.AddRange(guardian1, guardian2, guardian3);

        var athletes = new[]
        {
            new Athlete { TenantId = ClubTenantId, FirstName = "Olivia", LastName = "Wilson", DateOfBirth = new DateOnly(2013, 5, 15), Gender = "F", Level = CheerLevel.Level2, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10001" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Sophie", LastName = "Wilson", DateOfBirth = new DateOnly(2015, 8, 22), Gender = "F", Level = CheerLevel.Novice, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10002" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Jack", LastName = "Brown", DateOfBirth = new DateOnly(2012, 3, 10), Gender = "M", Level = CheerLevel.Level3, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10003" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Mia", LastName = "Brown", DateOfBirth = new DateOnly(2014, 11, 3), Gender = "F", Level = CheerLevel.Level2, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10004" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Charlie", LastName = "Jones", DateOfBirth = new DateOnly(2011, 7, 28), Gender = "M", Level = CheerLevel.Level4, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10005" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Ella", LastName = "Jones", DateOfBirth = new DateOnly(2009, 1, 14), Gender = "F", Level = CheerLevel.Level4, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10006" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Freya", LastName = "Smith", DateOfBirth = new DateOnly(2010, 4, 5), Gender = "F", Level = CheerLevel.Level3, HasMediaConsent = false, HasMedicalConsent = true, ExternalMembershipId = "SC-10007" },
            new Athlete { TenantId = ClubTenantId, FirstName = "Noah", LastName = "Taylor", DateOfBirth = new DateOnly(2013, 9, 18), Gender = "M", Level = CheerLevel.Level2, HasMediaConsent = true, HasMedicalConsent = true, ExternalMembershipId = "SC-10008" },
        };
        db.Athletes.AddRange(athletes);

        db.AthleteGuardians.AddRange(
            new AthleteGuardian { TenantId = ClubTenantId, AthleteId = athletes[0].Id, GuardianId = guardian1.Id, Relationship = "Mother", IsPrimaryContact = true },
            new AthleteGuardian { TenantId = ClubTenantId, AthleteId = athletes[1].Id, GuardianId = guardian1.Id, Relationship = "Mother", IsPrimaryContact = true },
            new AthleteGuardian { TenantId = ClubTenantId, AthleteId = athletes[2].Id, GuardianId = guardian2.Id, Relationship = "Father", IsPrimaryContact = true },
            new AthleteGuardian { TenantId = ClubTenantId, AthleteId = athletes[3].Id, GuardianId = guardian2.Id, Relationship = "Father", IsPrimaryContact = true },
            new AthleteGuardian { TenantId = ClubTenantId, AthleteId = athletes[4].Id, GuardianId = guardian3.Id, Relationship = "Mother", IsPrimaryContact = true },
            new AthleteGuardian { TenantId = ClubTenantId, AthleteId = athletes[5].Id, GuardianId = guardian3.Id, Relationship = "Mother", IsPrimaryContact = true });

        db.Enrolments.AddRange(
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[1].Id, ClassId = classes[0].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[0].Id, ClassId = classes[1].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[3].Id, ClassId = classes[1].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[7].Id, ClassId = classes[1].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[4].Id, ClassId = classes[2].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[5].Id, ClassId = classes[2].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[6].Id, ClassId = classes[2].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active },
            new Enrolment { TenantId = ClubTenantId, AthleteId = athletes[2].Id, ClassId = classes[3].Id, EnrolledDate = new DateOnly(2025, 9, 1), Status = EnrolmentStatus.Active });

        var team = new Team
        {
            TenantId = ClubTenantId,
            Name = "Stardust Novas",
            Level = CheerLevel.Level2,
            AgeGridDivision = "Junior",
            HeadCoachId = coach1Id
        };
        db.Teams.Add(team);

        db.TeamMembers.AddRange(
            new TeamMember { TenantId = ClubTenantId, TeamId = team.Id, AthleteId = athletes[0].Id, Position = "Flyer" },
            new TeamMember { TenantId = ClubTenantId, TeamId = team.Id, AthleteId = athletes[3].Id, Position = "Base" },
            new TeamMember { TenantId = ClubTenantId, TeamId = team.Id, AthleteId = athletes[7].Id, Position = "Base" },
            new TeamMember { TenantId = ClubTenantId, TeamId = team.Id, AthleteId = athletes[2].Id, Position = "Back Spot" });

        db.TeamMusic.Add(new TeamMusic
        {
            TenantId = ClubTenantId,
            TeamId = team.Id,
            FileName = "stardust_novas_routine.mp3",
            StoragePath = "/uploads/music/stardust_novas_routine.mp3",
            ContentType = "audio/mpeg",
            FileSizeBytes = 5_242_880,
            Duration = TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(30)),
            LicenceProof = "CC-LIC-2025-NOVAS-001",
            LicenceVerified = true,
            IsCurrent = true
        });

        var seniorTeam = new Team
        {
            TenantId = ClubTenantId,
            Name = "Stardust Supernovas",
            Level = CheerLevel.Level4,
            AgeGridDivision = "Senior",
            HeadCoachId = coach1Id
        };
        db.Teams.Add(seniorTeam);
        db.TeamMembers.AddRange(
            new TeamMember { TenantId = ClubTenantId, TeamId = seniorTeam.Id, AthleteId = athletes[4].Id, Position = "Base" },
            new TeamMember { TenantId = ClubTenantId, TeamId = seniorTeam.Id, AthleteId = athletes[5].Id, Position = "Flyer" },
            new TeamMember { TenantId = ClubTenantId, TeamId = seniorTeam.Id, AthleteId = athletes[6].Id, Position = "Base" });

        var camp = new Camp
        {
            TenantId = ClubTenantId,
            Name = "Summer Stunt Camp 2026",
            Description = "Intensive stunting skills camp for all levels",
            VenueId = venue.Id,
            StartDate = new DateOnly(2026, 7, 20),
            EndDate = new DateOnly(2026, 7, 22),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(16, 0),
            Capacity = 30,
            Price = 95m,
            Status = CampStatus.Published
        };
        db.Camps.Add(camp);

        await db.SaveChangesAsync();
    }

    private static async Task SeedCompetitionDataAsync(AppDbContext db)
    {
        var ageGrid = new AgeGrid
        {
            Name = "SportCheer UK 2025/26",
            Provider = "SportCheerUK",
            Season = 2025,
            Divisions = new List<AgeGridDivision>
            {
                new() { Name = "Tiny", MinAge = 4, MaxAge = 6 },
                new() { Name = "Mini", MinAge = 5, MaxAge = 8 },
                new() { Name = "Youth", MinAge = 7, MaxAge = 11 },
                new() { Name = "Junior", MinAge = 9, MaxAge = 14 },
                new() { Name = "Senior", MinAge = 11, MaxAge = 18 },
                new() { Name = "Open", MinAge = 14, MaxAge = 99 },
            }
        };
        db.AgeGrids.Add(ageGrid);

        var evt = new Event
        {
            TenantId = ProducerTenantId,
            Name = "UK Cheer Championship 2026",
            Description = "The premier UK cheerleading competition",
            VenueName = "NEC Birmingham",
            VenueAddress = "North Avenue, Marston Green, Birmingham B40 1NT",
            Status = EventStatus.EntriesOpen,
            EntryDeadline = new DateOnly(2026, 1, 15),
            BaseEntryFee = 175m
        };
        db.Events.Add(evt);

        var session1 = new EventSession { TenantId = ProducerTenantId, EventId = evt.Id, Name = "Day 1 - Juniors & Youth", Date = new DateOnly(2026, 2, 14), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0), SortOrder = 1 };
        var session2 = new EventSession { TenantId = ProducerTenantId, EventId = evt.Id, Name = "Day 2 - Seniors & Open", Date = new DateOnly(2026, 2, 15), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(18, 0), SortOrder = 2 };
        db.EventSessions.AddRange(session1, session2);

        var block1 = new SessionBlock { TenantId = ProducerTenantId, SessionId = session1.Id, Name = "Junior Level 2", StartTime = new TimeOnly(9, 30), SortOrder = 1 };
        var block2 = new SessionBlock { TenantId = ProducerTenantId, SessionId = session1.Id, Name = "Junior Level 3", StartTime = new TimeOnly(13, 0), SortOrder = 2 };
        var block3 = new SessionBlock { TenantId = ProducerTenantId, SessionId = session2.Id, Name = "Senior Level 4", StartTime = new TimeOnly(9, 30), SortOrder = 1 };
        db.SessionBlocks.AddRange(block1, block2, block3);

        var juniorGrid = ageGrid.Divisions.First(d => d.Name == "Junior");
        var seniorGrid = ageGrid.Divisions.First(d => d.Name == "Senior");

        var div1 = new Division { TenantId = ProducerTenantId, EventId = evt.Id, Name = "Junior Level 2", Level = CheerLevel.Level2, AgeGridId = ageGrid.Id, ScoresheetType = ScoresheetType.USS, MinTeamSize = 4, MaxTeamSize = 24, SortOrder = 1 };
        var div2 = new Division { TenantId = ProducerTenantId, EventId = evt.Id, Name = "Junior Level 3", Level = CheerLevel.Level3, AgeGridId = ageGrid.Id, ScoresheetType = ScoresheetType.USS, MinTeamSize = 4, MaxTeamSize = 24, SortOrder = 2 };
        var div3 = new Division { TenantId = ProducerTenantId, EventId = evt.Id, Name = "Senior Level 4", Level = CheerLevel.Level4, AgeGridId = ageGrid.Id, ScoresheetType = ScoresheetType.USS, MinTeamSize = 4, MaxTeamSize = 24, SortOrder = 3 };
        db.Divisions.AddRange(div1, div2, div3);

        // USS Scoresheet template for Junior Level 2
        var template = new ScoresheetTemplate { TenantId = ProducerTenantId, DivisionId = div1.Id, Name = "USS Standard", Type = ScoresheetType.USS };
        db.ScoresheetTemplates.Add(template);

        var stuntCaption = new ScoresheetCaption { TemplateId = template.Id, Name = "Stunts", MaxScore = 10, Weight = 1.0m, SortOrder = 1 };
        var pyramidCaption = new ScoresheetCaption { TemplateId = template.Id, Name = "Pyramids & Tosses", MaxScore = 10, Weight = 1.0m, SortOrder = 2 };
        var tumblingCaption = new ScoresheetCaption { TemplateId = template.Id, Name = "Standing & Running Tumbling", MaxScore = 10, Weight = 1.0m, SortOrder = 3 };
        var jumpCaption = new ScoresheetCaption { TemplateId = template.Id, Name = "Jumps", MaxScore = 10, Weight = 0.5m, SortOrder = 4 };
        var danceCaption = new ScoresheetCaption { TemplateId = template.Id, Name = "Dance/Motion", MaxScore = 10, Weight = 0.5m, SortOrder = 5 };
        var overallCaption = new ScoresheetCaption { TemplateId = template.Id, Name = "Overall Impression", MaxScore = 10, Weight = 1.0m, SortOrder = 6 };
        db.ScoresheetCaptions.AddRange(stuntCaption, pyramidCaption, tumblingCaption, jumpCaption, danceCaption, overallCaption);

        void AddSubcaptions(ScoresheetCaption caption, params string[] names)
        {
            int order = 1;
            foreach (var name in names)
            {
                db.ScoresheetSubcaptions.Add(new ScoresheetSubcaption
                {
                    CaptionId = caption.Id,
                    Name = name,
                    MinScore = 0,
                    MaxScore = 10,
                    Increment = 0.5m,
                    Weight = 1.0m,
                    SortOrder = order++
                });
            }
        }

        AddSubcaptions(stuntCaption, "Difficulty", "Execution");
        AddSubcaptions(pyramidCaption, "Difficulty", "Execution");
        AddSubcaptions(tumblingCaption, "Difficulty", "Execution");
        AddSubcaptions(jumpCaption, "Technique", "Difficulty");
        AddSubcaptions(danceCaption, "Technique", "Creativity");
        AddSubcaptions(overallCaption, "Performance", "Choreography");

        var panel = new JudgePanel { TenantId = ProducerTenantId, EventId = evt.Id, Name = "Panel A" };
        db.JudgePanels.Add(panel);
        db.JudgePanelMembers.AddRange(
            new JudgePanelMember { TenantId = ProducerTenantId, PanelId = panel.Id, JudgeUserId = "judge-1", JudgeName = "Helen Clarke", Role = "Head Judge" },
            new JudgePanelMember { TenantId = ProducerTenantId, PanelId = panel.Id, JudgeUserId = "judge-2", JudgeName = "Tom Harris", Role = "Judge", AssignedCaptionIndex = 0 },
            new JudgePanelMember { TenantId = ProducerTenantId, PanelId = panel.Id, JudgeUserId = "judge-3", JudgeName = "Rachel Green", Role = "Judge", AssignedCaptionIndex = 1 });

        await db.SaveChangesAsync();
    }
}
