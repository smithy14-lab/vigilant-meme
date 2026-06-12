namespace CheerDeck.Infrastructure.Services;

using CheerDeck.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

public class SendGridEmailService(IConfiguration configuration, ILogger<SendGridEmailService> logger) : IEmailService
{
    private string ApiKey => configuration["SendGrid:ApiKey"] ?? "";
    private string FromEmail => configuration["SendGrid:FromEmail"] ?? "noreply@cheerdeck.com";
    private string FromName => configuration["SendGrid:FromName"] ?? "CheerDeck";

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        await SendAsync([to], subject, htmlBody, ct);
    }

    public async Task SendAsync(IEnumerable<string> to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            logger.LogWarning("SendGrid API key not configured — email not sent. Subject: {Subject}", subject);
            return;
        }

        var personalizations = to.Select(email => new
        {
            to = new[] { new { email } }
        }).ToArray();

        var payload = new
        {
            personalizations,
            from = new { email = FromEmail, name = FromName },
            subject,
            content = new[] { new { type = "text/html", value = htmlBody } }
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.sendgrid.com/v3/mail/send", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("SendGrid API error ({Status}): {Body}", response.StatusCode, errorBody);
        }
        else
        {
            logger.LogInformation("Email sent via SendGrid. Subject: {Subject}, Recipients: {Count}", subject, personalizations.Length);
        }
    }

    public async Task SendTemplateAsync(string to, string templateId, Dictionary<string, string> substitutions, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            logger.LogWarning("SendGrid API key not configured — template email not sent. Template: {Template}", templateId);
            return;
        }

        var dynamicData = substitutions.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);

        var payload = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = to } },
                    dynamic_template_data = dynamicData
                }
            },
            from = new { email = FromEmail, name = FromName },
            template_id = templateId
        };

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.sendgrid.com/v3/mail/send", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            logger.LogError("SendGrid template error ({Status}): {Body}", response.StatusCode, errorBody);
        }
    }
}
