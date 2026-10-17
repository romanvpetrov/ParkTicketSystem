namespace TicketSystem.Domain.Repositories;

public interface IEmailService
{
    Task SendAsync(
        string senderEmail,
        string receiverEmail,
        string subject,
        string plainText,
        string html,
        CancellationToken cancellationToken = default);
}
