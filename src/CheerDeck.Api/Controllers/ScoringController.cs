using CheerDeck.Application.Services;
using CheerDeck.Domain.Competition;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScoringController(ScoringService scoringService) : ControllerBase
{
    public record TabulateRequest(Guid EntryId, Guid DivisionId);

    [HttpPost("score")]
    public async Task<IActionResult> SubmitScore([FromBody] Score score, CancellationToken ct)
    {
        var result = await scoringService.SubmitScoreAsync(score, ct);
        return Ok(result);
    }

    [HttpPost("sync-offline")]
    public async Task<IActionResult> SyncOffline([FromBody] List<Score> scores, CancellationToken ct)
    {
        await scoringService.SyncOfflineScoresAsync(scores, ct);
        return NoContent();
    }

    [HttpPost("deduction")]
    public async Task<IActionResult> AddDeduction([FromBody] Deduction deduction, CancellationToken ct)
    {
        var result = await scoringService.AddDeductionAsync(deduction, ct);
        return Ok(result);
    }

    [HttpPost("tabulate")]
    public async Task<IActionResult> Tabulate([FromBody] TabulateRequest request, CancellationToken ct)
    {
        var result = await scoringService.TabulateAsync(request.EntryId, request.DivisionId, ct);
        return Ok(result);
    }

    [HttpPost("rank/{divisionId:guid}")]
    public async Task<IActionResult> Rank(Guid divisionId, CancellationToken ct)
    {
        await scoringService.RankDivisionAsync(divisionId, ct);
        return NoContent();
    }

    [HttpPost("release/{divisionId:guid}")]
    public async Task<IActionResult> Release(Guid divisionId, CancellationToken ct)
    {
        await scoringService.ReleaseScoresAsync(divisionId, ct);
        return NoContent();
    }

    [HttpGet("results/{divisionId:guid}")]
    public async Task<IActionResult> GetResults(Guid divisionId, [FromQuery] bool releasedOnly = false, CancellationToken ct = default)
        => Ok(await scoringService.GetResultsAsync(divisionId, releasedOnly, ct));
}
