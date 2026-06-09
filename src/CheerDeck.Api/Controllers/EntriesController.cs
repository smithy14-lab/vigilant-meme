using CheerDeck.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntriesController(EntryService entryService) : ControllerBase
{
    public record SubmitEntryRequest(Guid EventId, Guid DivisionId, Guid TeamId, Guid? MusicFileId);
    public record ConfirmPaymentRequest(string PaymentId);

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitEntryRequest request, CancellationToken ct)
    {
        var entry = await entryService.SubmitEntryAsync(
            request.EventId, request.DivisionId, request.TeamId, request.MusicFileId, ct);
        return Ok(entry);
    }

    [HttpGet("by-event/{eventId:guid}")]
    public async Task<IActionResult> GetByEvent(Guid eventId, CancellationToken ct)
        => Ok(await entryService.GetEntriesForEventAsync(eventId, ct));

    [HttpPost("{id:guid}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] ConfirmPaymentRequest request, CancellationToken ct)
    {
        await entryService.ConfirmPaymentAsync(id, request.PaymentId, ct);
        return NoContent();
    }
}
