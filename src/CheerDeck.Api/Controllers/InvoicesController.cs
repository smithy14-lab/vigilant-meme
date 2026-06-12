using CheerDeck.Application.Services;
using CheerDeck.Domain.ClubManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class InvoicesController(InvoiceService invoiceService) : ControllerBase
{
    public record MarkPaidRequest(string PaymentId);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await invoiceService.GetAllAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Invoice invoice, CancellationToken ct)
    {
        var created = await invoiceService.CreateAsync(invoice, ct);
        return Ok(created);
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay(Guid id, CancellationToken ct)
    {
        var intent = await invoiceService.InitiatePaymentAsync(id, ct);
        return Ok(intent);
    }

    [HttpPost("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkPaid(Guid id, [FromBody] MarkPaidRequest request, CancellationToken ct)
    {
        await invoiceService.MarkPaidAsync(id, request.PaymentId, ct);
        return NoContent();
    }
}
