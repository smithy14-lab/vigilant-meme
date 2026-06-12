using CheerDeck.Application.Services;
using CheerDeck.Domain.Competition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RunningOrderController(RunningOrderService runningOrderService) : ControllerBase
{
    public record AddToRunningOrderRequest(Guid BlockId, Guid EntryId, int Position, TimeOnly? ScheduledTime);
    public record UpdateStatusRequest(PerformanceStatus Status);
    public record ReorderRequest(List<Guid> OrderedEntryIds);
    public record AssignWarmUpRequest(TimeOnly Start, TimeOnly End, string? Area);

    [HttpGet("by-block/{blockId:guid}")]
    public async Task<IActionResult> GetByBlock(Guid blockId, CancellationToken ct)
        => Ok(await runningOrderService.GetForBlockAsync(blockId, ct));

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddToRunningOrderRequest request, CancellationToken ct)
    {
        var roe = await runningOrderService.AddToRunningOrderAsync(
            request.BlockId, request.EntryId, request.Position, request.ScheduledTime, ct);
        return Ok(roe);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request, CancellationToken ct)
    {
        await runningOrderService.UpdateStatusAsync(id, request.Status, ct);
        return NoContent();
    }

    [HttpPut("block/{blockId:guid}/reorder")]
    public async Task<IActionResult> Reorder(Guid blockId, [FromBody] ReorderRequest request, CancellationToken ct)
    {
        await runningOrderService.ReorderAsync(blockId, request.OrderedEntryIds, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/warm-up")]
    public async Task<IActionResult> AssignWarmUp(Guid id, [FromBody] AssignWarmUpRequest request, CancellationToken ct)
    {
        await runningOrderService.AssignWarmUpAsync(id, request.Start, request.End, request.Area, ct);
        return NoContent();
    }
}
