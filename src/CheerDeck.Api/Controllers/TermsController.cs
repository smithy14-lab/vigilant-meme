using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TermsController(TermService termService) : ControllerBase
{
    public record DuplicateTermRequest(string NewName, DateOnly NewStart, DateOnly NewEnd);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await termService.GetAllAsync(ct));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var term = await termService.GetActiveAsync(ct);
        return term is null ? NotFound() : Ok(term);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Term term, CancellationToken ct)
    {
        var created = await termService.CreateAsync(term, ct);
        return CreatedAtAction(nameof(GetActive), created);
    }

    [HttpPost("duplicate/{id:guid}")]
    public async Task<IActionResult> Duplicate(Guid id, [FromBody] DuplicateTermRequest request, CancellationToken ct)
    {
        var newTerm = await termService.DuplicateAsync(id, request.NewName, request.NewStart, request.NewEnd, ct);
        return Ok(newTerm);
    }
}
