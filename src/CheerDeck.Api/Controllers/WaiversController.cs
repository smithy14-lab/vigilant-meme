using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WaiversController(WaiverService waiverService) : ControllerBase
{
    public record SignRequest(Guid GuardianId, Guid? AthleteId, string SignedByName);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await waiverService.GetAllAsync(ct));

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
        => Ok(await waiverService.GetActiveAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var waiver = await waiverService.GetByIdAsync(id, ct);
        return waiver is null ? NotFound() : Ok(waiver);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Waiver waiver, CancellationToken ct)
    {
        var created = await waiverService.CreateAsync(waiver, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{waiverId:guid}/sign")]
    public async Task<IActionResult> Sign(Guid waiverId, [FromBody] SignRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var signature = await waiverService.SignAsync(waiverId, request.GuardianId, request.AthleteId, request.SignedByName, ipAddress, ct);
        return Ok(signature);
    }

    [HttpGet("guardian/{guardianId:guid}/unsigned")]
    public async Task<IActionResult> GetUnsigned(Guid guardianId, CancellationToken ct)
        => Ok(await waiverService.GetUnsignedForGuardianAsync(guardianId, ct));

    [HttpGet("guardian/{guardianId:guid}/signatures")]
    public async Task<IActionResult> GetSignatures(Guid guardianId, CancellationToken ct)
        => Ok(await waiverService.GetSignaturesForGuardianAsync(guardianId, ct));
}
