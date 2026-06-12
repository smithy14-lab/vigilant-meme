using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class GuardiansController(GuardianService guardianService) : ControllerBase
{
    public record LinkAthleteRequest(Guid AthleteId, string Relationship, bool IsPrimary);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await guardianService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var guardian = await guardianService.GetByIdAsync(id, ct);
        return guardian is null ? NotFound() : Ok(guardian);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Guardian guardian, CancellationToken ct)
    {
        var created = await guardianService.CreateAsync(guardian, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/link-athlete")]
    public async Task<IActionResult> LinkAthlete(Guid id, [FromBody] LinkAthleteRequest request, CancellationToken ct)
    {
        await guardianService.LinkAthleteAsync(id, request.AthleteId, request.Relationship, request.IsPrimary, ct);
        return NoContent();
    }
}
