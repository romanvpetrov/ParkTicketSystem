namespace TicketSystem.Infrastructure.Shopping;

public sealed record FakeEmailMessage(
    string SenderEmail,
    string ReceiverEmail,
    string Subject,
    string PlainText,
    string Html);
