namespace CheerDeck.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendTemplateAsync(string to, string templateId, Dictionary<string, string> substitutions, CancellationToken ct = default);
}
