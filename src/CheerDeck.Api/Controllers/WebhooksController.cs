using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CheerDeck.Application.Interfaces;
using CheerDeck.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController(
    IConfiguration configuration,
    InvoiceService invoiceService,
    NotificationService notifications,
    IAppDbContext db,
    ILogger<WebhooksController> logger) : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var signatureHeader = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            logger.LogWarning("Stripe webhook secret not configured, rejecting request");
            return BadRequest("Webhook not configured");
        }

        if (!VerifySignature(payload, signatureHeader, webhookSecret))
        {
            logger.LogWarning("Invalid Stripe webhook signature");
            return Unauthorized("Invalid signature");
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var eventType = root.GetProperty("type").GetString();

            logger.LogInformation("Processing Stripe webhook: {EventType}", eventType);

            var dataObj = root.GetProperty("data").GetProperty("object");

            switch (eventType)
            {
                case "checkout.session.completed":
                    var sessionId = dataObj.GetProperty("id").GetString();
                    var sessionMetadata = dataObj.GetProperty("metadata");
                    var paymentStatus = dataObj.TryGetProperty("payment_status", out var psProp) ? psProp.GetString() : null;

                    if (paymentStatus == "paid")
                    {
                        if (sessionMetadata.TryGetProperty("invoiceId", out var csInvoiceIdProp) &&
                            Guid.TryParse(csInvoiceIdProp.GetString(), out var csInvoiceId))
                        {
                            await invoiceService.MarkPaidAsync(csInvoiceId, sessionId!, ct);
                            logger.LogInformation("Invoice {InvoiceId} marked as paid via checkout session", csInvoiceId);
                        }

                        if (sessionMetadata.TryGetProperty("entryId", out var entryIdProp) &&
                            Guid.TryParse(entryIdProp.GetString(), out var entryId))
                        {
                            var entry = await db.EventEntries.FindAsync(new object[] { entryId }, ct);
                            if (entry is not null)
                            {
                                entry.Status = CheerDeck.Domain.Competition.EntryStatus.Confirmed;
                                entry.PaymentId = sessionId;
                                entry.PaidAt = DateTime.UtcNow;
                                await db.SaveChangesAsync(ct);
                                logger.LogInformation("Entry {EntryId} confirmed as paid via checkout session", entryId);
                            }
                        }
                    }
                    break;

                case "customer.subscription.updated":
                case "customer.subscription.deleted":
                    var subMetadata = dataObj.GetProperty("metadata");
                    if (subMetadata.TryGetProperty("tenantId", out var tenantIdProp) &&
                        Guid.TryParse(tenantIdProp.GetString(), out var tenantId))
                    {
                        var tenant = await db.Tenants.FindAsync(new object[] { tenantId }, ct);
                        if (tenant is not null)
                        {
                            var subStatus = dataObj.GetProperty("status").GetString();
                            tenant.SubscriptionStatus = subStatus switch
                            {
                                "active" => CheerDeck.Domain.Common.SubscriptionStatus.Active,
                                "past_due" => CheerDeck.Domain.Common.SubscriptionStatus.PastDue,
                                "canceled" or "cancelled" => CheerDeck.Domain.Common.SubscriptionStatus.Cancelled,
                                "trialing" => CheerDeck.Domain.Common.SubscriptionStatus.Trialing,
                                _ => tenant.SubscriptionStatus
                            };

                            if (dataObj.TryGetProperty("current_period_end", out var periodEnd))
                            {
                                var endEpoch = periodEnd.GetInt64();
                                tenant.SubscriptionEndDate = DateTimeOffset.FromUnixTimeSeconds(endEpoch).UtcDateTime;
                            }

                            tenant.UpdatedAt = DateTime.UtcNow;
                            await db.SaveChangesAsync(ct);
                            logger.LogInformation("Tenant {TenantId} subscription updated to {Status}", tenantId, subStatus);
                        }
                    }
                    break;

                case "payment_intent.succeeded":
                    var paymentId = dataObj.GetProperty("id").GetString();
                    var metadata = dataObj.GetProperty("metadata");
                    if (metadata.TryGetProperty("invoiceId", out var invoiceIdProp) &&
                        Guid.TryParse(invoiceIdProp.GetString(), out var invoiceId))
                    {
                        await invoiceService.MarkPaidAsync(invoiceId, paymentId!, ct);
                        logger.LogInformation("Invoice {InvoiceId} marked as paid via webhook", invoiceId);

                        if (metadata.TryGetProperty("email", out var emailProp))
                        {
                            var amount = dataObj.GetProperty("amount").GetInt64() / 100m;
                            await notifications.SendPaymentReceiptAsync(
                                emailProp.GetString()!, "Invoice payment", amount, paymentId!, ct);
                        }
                    }
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing Stripe webhook");
            return StatusCode(500);
        }
    }

    private static bool VerifySignature(string payload, string signatureHeader, string secret)
    {
        if (string.IsNullOrEmpty(signatureHeader))
            return false;

        var parts = signatureHeader.Split(',')
            .Select(p => p.Trim().Split('=', 2))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);

        if (!parts.TryGetValue("t", out var timestamp) || !parts.TryGetValue("v1", out var expectedSig))
            return false;

        var signedPayload = $"{timestamp}.{payload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSig = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSig),
            Encoding.UTF8.GetBytes(expectedSig));
    }
}
