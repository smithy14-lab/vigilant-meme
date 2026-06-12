using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AthletesController(AthleteService athleteService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await athleteService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var athlete = await athleteService.GetByIdAsync(id, ct);
        return athlete is null ? NotFound() : Ok(athlete);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct)
        => Ok(await athleteService.SearchAsync(q, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Athlete athlete, CancellationToken ct)
    {
        var created = await athleteService.CreateAsync(athlete, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Athlete athlete, CancellationToken ct)
    {
        athlete.Id = id;
        var updated = await athleteService.UpdateAsync(athlete, ct);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken ct)
    {
        await athleteService.SoftDeleteAsync(id, ct);
        return NoContent();
    }
}
