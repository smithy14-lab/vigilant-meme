using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CheerDeck.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController(
    IConfiguration configuration,
    InvoiceService invoiceService,
    NotificationService notifications,
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

            switch (eventType)
            {
                case "payment_intent.succeeded":
                    var paymentId = root.GetProperty("data").GetProperty("object").GetProperty("id").GetString();
                    var metadata = root.GetProperty("data").GetProperty("object").GetProperty("metadata");
                    if (metadata.TryGetProperty("invoiceId", out var invoiceIdProp) &&
                        Guid.TryParse(invoiceIdProp.GetString(), out var invoiceId))
                    {
                        await invoiceService.MarkPaidAsync(invoiceId, paymentId!, ct);
                        logger.LogInformation("Invoice {InvoiceId} marked as paid via webhook", invoiceId);

                        if (metadata.TryGetProperty("email", out var emailProp))
                        {
                            var amount = root.GetProperty("data").GetProperty("object").GetProperty("amount").GetInt64() / 100m;
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
