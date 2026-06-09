using CheerDeck.Application.Services;
using CheerDeck.Domain.Competition;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(EventService eventService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await eventService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var evt = await eventService.GetByIdAsync(id, ct);
        return evt is null ? NotFound() : Ok(evt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Event evt, CancellationToken ct)
    {
        var created = await eventService.CreateAsync(evt, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id:guid}/divisions")]
    public async Task<IActionResult> AddDivision(Guid id, [FromBody] Division division, CancellationToken ct)
    {
        var created = await eventService.AddDivisionAsync(id, division, ct);
        return Ok(created);
    }

    [HttpPost("{id:guid}/sessions")]
    public async Task<IActionResult> AddSession(Guid id, [FromBody] EventSession session, CancellationToken ct)
    {
        var created = await eventService.AddSessionAsync(id, session, ct);
        return Ok(created);
    }
}
