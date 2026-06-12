using Microsoft.AspNetCore.Mvc;

namespace CheerDeck.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhooksController(
    IConfiguration configuration,
    ILogger<WebhooksController> logger) : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var payload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        var webhookSecret = configuration["Stripe:WebhookSecret"];

        if (string.IsNullOrEmpty(webhookSecret))
        {
            logger.LogWarning("Stripe webhook secret not configured, skipping verification");
            return Ok();
        }

        logger.LogInformation("Received Stripe webhook event");

        return Ok();
    }
}
