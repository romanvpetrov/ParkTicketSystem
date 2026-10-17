namespace TicketSystem.Domain.Orders;

public sealed record CartPaymentResult(
    bool Success,
    IReadOnlyList<Ticket> Tickets,
    string? FailureCode)
{
    public static CartPaymentResult Succeeded(IReadOnlyList<Ticket> tickets)
    {
        return new CartPaymentResult(true, tickets, null);
    }

    public static CartPaymentResult Failed(string failureCode)
    {
        return new CartPaymentResult(false, [], failureCode);
    }
}
