namespace CheerDeck.Infrastructure.Services;

using CheerDeck.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

public class BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger) : IEmailService
{
    private string ApiKey => configuration["Brevo:ApiKey"] ?? "";
    private string FromEmail => configuration["Brevo:FromEmail"] ?? "noreply@cheerdeck.com";
    private string FromName => configuration["Brevo:FromName"] ?? "CheerDeck";

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        await SendAsync([to], subject, htmlBody, ct);
    }

    public async Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            logger.LogWarning("Brevo API key not configured — email not sent. Subject: {Subject}", subject);
            return;
        }

        var recipients = to.Select(email => new { email }).ToArray();

        var payload = new
        {
            sender = new { email = FromEmail, name = FromName },
            to = recipients,
            subject,
            htmlContent = htmlBody
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", ApiKey);
        client.DefaultRequestHeaders.Add("accept", "application/json");

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Brevo API error ({Status}): {Body}", response.StatusCode, errorBody);
        }
        else
        {
            logger.LogInformation("Email sent via Brevo. Subject: {Subject}, Recipients: {Count}", subject, recipients.Length);
        }
    }

    public async Task SendTemplateAsync(string to, string templateId, Dictionary<string, string> substitutions, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            logger.LogWarning("Brevo API key not configured — template email not sent. Template: {Template}", templateId);
            return;
        }

        if (!long.TryParse(templateId, out var templateNumber))
        {
            logger.LogError("Invalid Brevo template ID: {TemplateId}", templateId);
            return;
        }

        var payload = new
        {
            sender = new { email = FromEmail, name = FromName },
            to = new[] { new { email = to } },
            templateId = templateNumber,
            @params = substitutions
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", ApiKey);
        client.DefaultRequestHeaders.Add("accept", "application/json");

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Brevo template error ({Status}): {Body}", response.StatusCode, errorBody);
        }
    }
}
