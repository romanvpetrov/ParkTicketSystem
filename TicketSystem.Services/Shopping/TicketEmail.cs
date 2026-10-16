namespace TicketSystem.Services.Shopping;

public sealed record TicketEmail(
    string SenderEmail,
    string ReceiverEmail,
    string Subject,
    string PlainText,
    string Html);
