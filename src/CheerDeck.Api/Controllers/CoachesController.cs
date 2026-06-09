using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoachesController(CoachService coachService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await coachService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var coach = await coachService.GetByIdAsync(id, ct);
        return coach is null ? NotFound() : Ok(coach);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Coach coach, CancellationToken ct)
    {
        var created = await coachService.CreateAsync(coach, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("expiring-credentials")]
    public async Task<IActionResult> GetExpiringCredentials([FromQuery] int daysAhead = 30, CancellationToken ct = default)
        => Ok(await coachService.GetWithExpiringCredentialsAsync(daysAhead, ct));
}
