namespace CheerDeck.Infrastructure.Stubs;

using CheerDeck.Application.Interfaces;
using Microsoft.Extensions.Logging;

public class StubEmailService(ILogger<StubEmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation("[StubEmail] To: {To}, Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation("[StubEmail] To: {To}, Subject: {Subject}", string.Join(", ", to), subject);
        return Task.CompletedTask;
    }

    public Task SendTemplateAsync(string to, string templateId, Dictionary<string, string> substitutions, CancellationToken ct = default)
    {
        logger.LogInformation("[StubEmail] Template: {Template}, To: {To}", templateId, to);
        return Task.CompletedTask;
    }
}
