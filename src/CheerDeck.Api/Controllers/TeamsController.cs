using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TeamsController(TeamService teamService) : ControllerBase
{
    public record AddMemberRequest(Guid AthleteId, string? Position, bool IsAlternate);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await teamService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var team = await teamService.GetByIdAsync(id, ct);
        return team is null ? NotFound() : Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Team team, CancellationToken ct)
    {
        var created = await teamService.CreateAsync(team, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest request, CancellationToken ct)
    {
        await teamService.AddMemberAsync(id, request.AthleteId, request.Position, request.IsAlternate, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}/members/{athleteId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid athleteId, CancellationToken ct)
    {
        await teamService.RemoveMemberAsync(id, athleteId, ct);
        return NoContent();
    }
}
