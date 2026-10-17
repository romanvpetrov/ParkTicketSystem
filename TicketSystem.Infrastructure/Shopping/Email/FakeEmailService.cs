using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class FakeEmailService : IEmailService
{
    private readonly List<FakeEmailMessage> _sentEmails = [];

    public IReadOnlyList<FakeEmailMessage> SentEmails => _sentEmails;

    public Task SendAsync(
        string senderEmail,
        string receiverEmail,
        string subject,
        string plainText,
        string html,
        CancellationToken cancellationToken = default)
    {
        _sentEmails.Add(new FakeEmailMessage(senderEmail, receiverEmail, subject, plainText, html));
        return Task.CompletedTask;
    }
}
