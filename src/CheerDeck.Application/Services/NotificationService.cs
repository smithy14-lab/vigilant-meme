namespace CheerDeck.Application.Services;

using CheerDeck.Application.Interfaces;

public class NotificationService(IEmailService email)
{
    public async Task SendRegistrationConfirmationAsync(string toEmail, string athleteName, string? guardianName, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Registration Confirmed</h2>
            <p>Hi{(guardianName != null ? $" {guardianName}" : "")},</p>
            <p><strong>{athleteName}</strong> has been successfully registered with our club.</p>
            <p>You can log in to your parent portal to view your athlete's details, upcoming classes, and invoices.</p>
            <p>Welcome to the team!</p>
            <p><em>CheerDeck Club</em></p>
            """;
        await email.SendAsync(toEmail, $"Registration Confirmed - {athleteName}", html, ct);
    }

    public async Task SendBookingConfirmationAsync(string toEmail, string athleteName, string className, string dayTime, decimal price, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Class Booking Confirmed</h2>
            <p><strong>{athleteName}</strong> has been enrolled in <strong>{className}</strong>.</p>
            <p><strong>Schedule:</strong> {dayTime}</p>
            <p><strong>Fee:</strong> &pound;{price:F2}</p>
            <p>An invoice has been generated and is available in your parent portal.</p>
            <p><em>CheerDeck Club</em></p>
            """;
        await email.SendAsync(toEmail, $"Booking Confirmed - {className}", html, ct);
    }

    public async Task SendInvoiceNotificationAsync(string toEmail, string guardianName, string invoiceRef, decimal total, CancellationToken ct = default)
    {
        var html = $"""
            <h2>New Invoice</h2>
            <p>Hi {guardianName},</p>
            <p>A new invoice <strong>{invoiceRef}</strong> has been created for <strong>&pound;{total:F2}</strong>.</p>
            <p>Please log in to your parent portal to view and pay the invoice.</p>
            <p><em>CheerDeck Club</em></p>
            """;
        await email.SendAsync(toEmail, $"Invoice {invoiceRef} - {total:C}", html, ct);
    }

    public async Task SendEntryConfirmationAsync(string toEmail, string teamName, string eventName, string divisionName, string entryNumber, decimal fee, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Competition Entry Submitted</h2>
            <p>Your entry has been submitted successfully.</p>
            <p><strong>Team:</strong> {teamName}</p>
            <p><strong>Event:</strong> {eventName}</p>
            <p><strong>Division:</strong> {divisionName}</p>
            <p><strong>Entry Number:</strong> {entryNumber}</p>
            <p><strong>Entry Fee:</strong> &pound;{fee:F2}</p>
            <p>You will receive further information as the event date approaches.</p>
            <p><em>CheerDeck Competitions</em></p>
            """;
        await email.SendAsync(toEmail, $"Entry Confirmed - {teamName} - {eventName}", html, ct);
    }

    public async Task SendWaiverReminderAsync(string toEmail, string guardianName, int unsignedCount, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Waiver Reminder</h2>
            <p>Hi {guardianName},</p>
            <p>You have <strong>{unsignedCount}</strong> unsigned waiver(s) that require your attention.</p>
            <p>Please log in to your parent portal to review and sign them.</p>
            <p><em>CheerDeck Club</em></p>
            """;
        await email.SendAsync(toEmail, $"Action Required: {unsignedCount} Unsigned Waiver(s)", html, ct);
    }

    public async Task SendPaymentReceiptAsync(string toEmail, string description, decimal amount, string paymentId, CancellationToken ct = default)
    {
        var html = $"""
            <h2>Payment Receipt</h2>
            <p>Thank you for your payment.</p>
            <p><strong>Description:</strong> {description}</p>
            <p><strong>Amount:</strong> &pound;{amount:F2}</p>
            <p><strong>Reference:</strong> {paymentId}</p>
            <p><em>CheerDeck</em></p>
            """;
        await email.SendAsync(toEmail, $"Payment Receipt - {amount:C}", html, ct);
    }
}
