namespace CheerDeck.Infrastructure.Stubs;

using CheerDeck.Domain.Integration;

public class StubPaymentGateway : IPaymentGateway
{
    public Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string description,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        var id = $"pi_stub_{Guid.NewGuid():N}".Substring(0, 24);
        return Task.FromResult(new PaymentIntent(
            Id: id,
            Status: "requires_confirmation",
            Amount: amount,
            Currency: currency,
            ClientSecret: $"{id}_secret_stub",
            CheckoutUrl: null));
    }

    public Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentResult(
            Success: true,
            PaymentId: paymentIntentId,
            Status: "succeeded",
            FailureReason: null));
    }

    public Task<RefundResult> RefundPaymentAsync(string paymentId, decimal? amount = null, CancellationToken ct = default)
    {
        return Task.FromResult(new RefundResult(
            Success: true,
            RefundId: $"re_stub_{Guid.NewGuid():N}".Substring(0, 24),
            Amount: amount ?? 0,
            FailureReason: null));
    }

    public Task<SubscriptionResult> CreateSubscriptionAsync(string customerId, string priceId,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default)
    {
        return Task.FromResult(new SubscriptionResult(
            Success: true,
            SubscriptionId: $"sub_stub_{Guid.NewGuid():N}".Substring(0, 24),
            Status: "active",
            FailureReason: null));
    }

    public Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default)
    {
        return Task.FromResult(new SubscriptionResult(
            Success: true,
            SubscriptionId: subscriptionId,
            Status: "cancelled",
            FailureReason: null));
    }
}
