namespace CheerDeck.Infrastructure.Services;

using CheerDeck.Domain.Integration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class StripePaymentGateway(IConfiguration configuration, ILogger<StripePaymentGateway> logger) : IPaymentGateway
{
    private string SecretKey => configuration["Stripe:SecretKey"] ?? "";

    public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string description,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(SecretKey))
        {
            logger.LogWarning("Stripe secret key not configured — using stub mode");
            return new PaymentIntent($"pi_stub_{Guid.NewGuid():N}"[..24], "requires_confirmation", amount, currency, null, null);
        }

        logger.LogInformation("Creating Stripe PaymentIntent: {Amount} {Currency}", amount, currency);

        // Stripe API call via HttpClient (avoiding direct Stripe NuGet for now)
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SecretKey);

        var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["amount"] = ((int)(amount * 100)).ToString(),
            ["currency"] = currency.ToLower(),
            ["description"] = description
        });

        if (metadata != null)
        {
            var allParams = new Dictionary<string, string>
            {
                ["amount"] = ((int)(amount * 100)).ToString(),
                ["currency"] = currency.ToLower(),
                ["description"] = description
            };
            foreach (var (key, value) in metadata)
                allParams[$"metadata[{key}]"] = value;
            formContent = new FormUrlEncodedContent(allParams);
        }

        var response = await client.PostAsync("https://api.stripe.com/v1/payment_intents", formContent, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Stripe API error: {Response}", json);
            return new PaymentIntent("", "error", amount, currency, null, null);
        }

        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        return new PaymentIntent(
            Id: root.GetProperty("id").GetString()!,
            Status: root.GetProperty("status").GetString()!,
            Amount: amount,
            Currency: currency,
            ClientSecret: root.GetProperty("client_secret").GetString(),
            CheckoutUrl: null);
    }

    public async Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(SecretKey))
            return new PaymentResult(true, paymentIntentId, "succeeded", null);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SecretKey);

        var response = await client.PostAsync($"https://api.stripe.com/v1/payment_intents/{paymentIntentId}/confirm", null, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Stripe confirm error: {Response}", json);
            return new PaymentResult(false, paymentIntentId, "failed", json);
        }

        var doc = System.Text.Json.JsonDocument.Parse(json);
        return new PaymentResult(true, paymentIntentId, doc.RootElement.GetProperty("status").GetString()!, null);
    }

    public async Task<RefundResult> RefundPaymentAsync(string paymentId, decimal? amount = null, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(SecretKey))
            return new RefundResult(true, $"re_stub_{Guid.NewGuid():N}"[..24], amount ?? 0, null);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SecretKey);

        var parameters = new Dictionary<string, string> { ["payment_intent"] = paymentId };
        if (amount.HasValue)
            parameters["amount"] = ((int)(amount.Value * 100)).ToString();

        var response = await client.PostAsync("https://api.stripe.com/v1/refunds", new FormUrlEncodedContent(parameters), ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Stripe refund error: {Response}", json);
            return new RefundResult(false, "", 0, json);
        }

        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        return new RefundResult(true, root.GetProperty("id").GetString()!, root.GetProperty("amount").GetInt32() / 100m, null);
    }

    public async Task<SubscriptionResult> CreateSubscriptionAsync(string customerId, string priceId,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(SecretKey))
            return new SubscriptionResult(true, $"sub_stub_{Guid.NewGuid():N}"[..24], "active", null);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SecretKey);

        var parameters = new Dictionary<string, string>
        {
            ["customer"] = customerId,
            ["items[0][price]"] = priceId
        };

        if (metadata != null)
            foreach (var (key, value) in metadata)
                parameters[$"metadata[{key}]"] = value;

        var response = await client.PostAsync("https://api.stripe.com/v1/subscriptions", new FormUrlEncodedContent(parameters), ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Stripe subscription error: {Response}", json);
            return new SubscriptionResult(false, "", "failed", json);
        }

        var doc = System.Text.Json.JsonDocument.Parse(json);
        var root = doc.RootElement;
        return new SubscriptionResult(true, root.GetProperty("id").GetString()!, root.GetProperty("status").GetString()!, null);
    }

    public async Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(SecretKey))
            return new SubscriptionResult(true, subscriptionId, "cancelled", null);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SecretKey);

        var response = await client.DeleteAsync($"https://api.stripe.com/v1/subscriptions/{subscriptionId}", ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Stripe cancel subscription error: {Response}", json);
            return new SubscriptionResult(false, subscriptionId, "failed", json);
        }

        return new SubscriptionResult(true, subscriptionId, "cancelled", null);
    }
}
