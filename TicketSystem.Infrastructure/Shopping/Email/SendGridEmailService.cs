using SendGrid;
using SendGrid.Helpers.Mail;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class SendGridEmailService : IEmailService
{
    private readonly SendGridClient _client;

    public SendGridEmailService(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("A SendGrid API key is required.", nameof(apiKey));
        }

        _client = new SendGridClient(apiKey);
    }

    public async Task SendAsync(
        string senderEmail,
        string receiverEmail,
        string subject,
        string plainText,
        string html,
        CancellationToken cancellationToken = default)
    {
        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(senderEmail),
            new EmailAddress(receiverEmail),
            subject,
            plainText,
            html);

        var response = await _client
            .SendEmailAsync(message, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException(
                $"SendGrid rejected the ticket email with status {(int)response.StatusCode}: {responseBody}");
        }
    }
}
