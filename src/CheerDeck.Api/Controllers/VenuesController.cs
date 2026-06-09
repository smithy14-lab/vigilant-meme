using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VenuesController(VenueService venueService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await venueService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var venue = await venueService.GetByIdAsync(id, ct);
        return venue is null ? NotFound() : Ok(venue);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Venue venue, CancellationToken ct)
    {
        var created = await venueService.CreateAsync(venue, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
