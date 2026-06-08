namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using Microsoft.EntityFrameworkCore;

public class TeamService(IAppDbContext db, ITenantContext tenant)
{
    public async Task<List<Team>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Teams
            .Include(t => t.Members).ThenInclude(m => m.Athlete)
            .Include(t => t.HeadCoach)
            .Include(t => t.Music)
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<Team?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Teams
            .Include(t => t.Members).ThenInclude(m => m.Athlete)
            .Include(t => t.HeadCoach).ThenInclude(c => c!.Qualifications)
            .Include(t => t.Music)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct);
    }

    public async Task<Team> CreateAsync(Team team, CancellationToken ct = default)
    {
        team.TenantId = tenant.TenantId;
        team.CreatedBy = tenant.UserId;
        db.Teams.Add(team);
        await db.SaveChangesAsync(ct);
        return team;
    }

    public async Task AddMemberAsync(Guid teamId, Guid athleteId, string? position = null, bool isAlternate = false, CancellationToken ct = default)
    {
        db.TeamMembers.Add(new TeamMember
        {
            TenantId = tenant.TenantId,
            TeamId = teamId,
            AthleteId = athleteId,
            Position = position,
            IsAlternate = isAlternate
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid athleteId, CancellationToken ct = default)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamId == teamId && m.AthleteId == athleteId, ct);
        if (member != null)
        {
            db.TeamMembers.Remove(member);
            await db.SaveChangesAsync(ct);
        }
    }
}
