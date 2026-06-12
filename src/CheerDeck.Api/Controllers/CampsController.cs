using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CampsController(CampService campService) : ControllerBase
{
    public record BookAthleteRequest(Guid AthleteId);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await campService.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Camp camp, CancellationToken ct)
    {
        var created = await campService.CreateAsync(camp, ct);
        return Ok(created);
    }

    [HttpPost("{id:guid}/book")]
    public async Task<IActionResult> Book(Guid id, [FromBody] BookAthleteRequest request, CancellationToken ct)
    {
        var booking = await campService.BookAthleteAsync(id, request.AthleteId, ct);
        return Ok(booking);
    }

    [HttpPost("{id:guid}/process-waiting-list")]
    public async Task<IActionResult> ProcessWaitingList(Guid id, CancellationToken ct)
    {
        await campService.ProcessWaitingListAsync(id, ct);
        return NoContent();
    }
}
