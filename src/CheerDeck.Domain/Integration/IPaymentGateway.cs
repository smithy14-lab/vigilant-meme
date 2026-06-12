namespace CheerDeck.Domain.Integration;

public record CheckoutSession(string Id, string Url);

public record PaymentIntent(
    string Id,
    string Status,
    decimal Amount,
    string Currency,
    string? ClientSecret,
    string? CheckoutUrl);

public record PaymentResult(
    bool Success,
    string PaymentId,
    string Status,
    string? FailureReason);

public record RefundResult(
    bool Success,
    string RefundId,
    decimal Amount,
    string? FailureReason);

public record SubscriptionResult(
    bool Success,
    string SubscriptionId,
    string Status,
    string? FailureReason);

public interface IPaymentGateway
{
    Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string description,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default);

    Task<PaymentResult> ConfirmPaymentAsync(string paymentIntentId, CancellationToken ct = default);

    Task<RefundResult> RefundPaymentAsync(string paymentId, decimal? amount = null, CancellationToken ct = default);

    Task<SubscriptionResult> CreateSubscriptionAsync(string customerId, string priceId,
        Dictionary<string, string>? metadata = null, CancellationToken ct = default);

    Task<SubscriptionResult> CancelSubscriptionAsync(string subscriptionId, CancellationToken ct = default);

    Task<CheckoutSession> CreateCheckoutSessionAsync(decimal amount, string currency, string description,
        string successUrl, string cancelUrl, Dictionary<string, string>? metadata = null, CancellationToken ct = default);
}
