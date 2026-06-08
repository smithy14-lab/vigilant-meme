namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;
using CheerDeck.Domain.ClubManagement;
using CheerDeck.Domain.Integration;
using Microsoft.EntityFrameworkCore;

public class InvoiceService(IAppDbContext db, ITenantContext tenant, IPaymentGateway payments)
{
    public async Task<List<Invoice>> GetAllAsync(CancellationToken ct = default)
    {
        return await db.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.Guardian)
            .Include(i => i.Coach)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(ct);
    }

    public async Task<Invoice> CreateAsync(Invoice invoice, CancellationToken ct = default)
    {
        invoice.TenantId = tenant.TenantId;
        invoice.CreatedBy = tenant.UserId;
        invoice.InvoiceNumber = await GenerateInvoiceNumberAsync(ct);
        invoice.Subtotal = invoice.LineItems.Sum(li => li.Total);
        invoice.Total = invoice.Subtotal + invoice.TaxAmount;
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync(ct);
        return invoice;
    }

    public async Task<PaymentIntent> InitiatePaymentAsync(Guid invoiceId, CancellationToken ct = default)
    {
        var invoice = await db.Invoices.FindAsync([invoiceId], ct)
            ?? throw new InvalidOperationException("Invoice not found");

        var intent = await payments.CreatePaymentIntentAsync(
            invoice.Total, invoice.Currency,
            $"Invoice {invoice.InvoiceNumber}",
            new Dictionary<string, string> { ["invoiceId"] = invoiceId.ToString() }, ct);

        invoice.ExternalPaymentId = intent.Id;
        invoice.Status = InvoiceStatus.Sent;
        await db.SaveChangesAsync(ct);

        return intent;
    }

    public async Task MarkPaidAsync(Guid invoiceId, string paymentId, CancellationToken ct = default)
    {
        var invoice = await db.Invoices.FindAsync([invoiceId], ct)
            ?? throw new InvalidOperationException("Invoice not found");

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateOnly.FromDateTime(DateTime.UtcNow);
        invoice.ExternalPaymentId = paymentId;
        await db.SaveChangesAsync(ct);
    }

    private async Task<string> GenerateInvoiceNumberAsync(CancellationToken ct)
    {
        var count = await db.Invoices.CountAsync(ct);
        return $"INV-{count + 1:D6}";
    }
}
