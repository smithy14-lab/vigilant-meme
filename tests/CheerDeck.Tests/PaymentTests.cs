namespace CheerDeck.Tests;

using CheerDeck.Infrastructure.Stubs;
using FluentAssertions;

public class PaymentTests
{
    [Fact]
    public async Task Stub_Payment_Creates_Intent_With_Correct_Amount()
    {
        var gateway = new StubPaymentGateway();

        var intent = await gateway.CreatePaymentIntentAsync(175.00m, "GBP", "Competition entry");

        intent.Amount.Should().Be(175.00m);
        intent.Currency.Should().Be("GBP");
        intent.Status.Should().Be("requires_confirmation");
        intent.Id.Should().StartWith("pi_stub_");
    }

    [Fact]
    public async Task Stub_Payment_Confirms_Successfully()
    {
        var gateway = new StubPaymentGateway();

        var intent = await gateway.CreatePaymentIntentAsync(100m, "GBP", "Test");
        var result = await gateway.ConfirmPaymentAsync(intent.Id);

        result.Success.Should().BeTrue();
        result.PaymentId.Should().Be(intent.Id);
        result.Status.Should().Be("succeeded");
    }

    [Fact]
    public async Task Stub_Refund_Succeeds()
    {
        var gateway = new StubPaymentGateway();

        var result = await gateway.RefundPaymentAsync("pi_test", 50m);

        result.Success.Should().BeTrue();
        result.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task Stub_Subscription_Lifecycle()
    {
        var gateway = new StubPaymentGateway();

        var sub = await gateway.CreateSubscriptionAsync("cust_1", "price_monthly");
        sub.Success.Should().BeTrue();
        sub.Status.Should().Be("active");

        var cancel = await gateway.CancelSubscriptionAsync(sub.SubscriptionId);
        cancel.Success.Should().BeTrue();
        cancel.Status.Should().Be("cancelled");
    }
}
