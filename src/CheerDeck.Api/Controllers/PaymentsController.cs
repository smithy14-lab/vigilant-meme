using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.Integration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentGateway payments, IAppDbContext db) : ControllerBase
{
    public record CheckoutRequest(Guid InvoiceId, string SuccessUrl, string CancelUrl);
    public record EntryCheckoutRequest(Guid EntryId, string SuccessUrl, string CancelUrl);

    [HttpPost("checkout/invoice")]
    public async Task<IActionResult> CheckoutInvoice([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var invoice = await db.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, ct);

        if (invoice is null)
            return NotFound("Invoice not found");

        var session = await payments.CreateCheckoutSessionAsync(
            invoice.Total,
            invoice.Currency,
            $"Invoice {invoice.InvoiceNumber}",
            request.SuccessUrl,
            request.CancelUrl,
            new Dictionary<string, string> { ["invoiceId"] = invoice.Id.ToString() },
            ct);

        return Ok(new { session.Id, session.Url });
    }

    [HttpPost("checkout/entry")]
    public async Task<IActionResult> CheckoutEntry([FromBody] EntryCheckoutRequest request, CancellationToken ct)
    {
        var entry = await db.EventEntries
            .Include(e => e.Division)
                .ThenInclude(d => d.Event)
            .Include(e => e.Team)
            .FirstOrDefaultAsync(e => e.Id == request.EntryId, ct);

        if (entry is null)
            return NotFound("Entry not found");

        var currency = entry.Division?.Event?.Currency ?? "GBP";
        var description = $"Entry: {entry.Team?.Name ?? entry.TeamName ?? "Team"} - {entry.Division?.Name ?? "Division"}";

        var session = await payments.CreateCheckoutSessionAsync(
            entry.EntryFee,
            currency,
            description,
            request.SuccessUrl,
            request.CancelUrl,
            new Dictionary<string, string>
            {
                ["entryId"] = entry.Id.ToString(),
                ["eventId"] = entry.EventId.ToString()
            },
            ct);

        return Ok(new { session.Id, session.Url });
    }
}
