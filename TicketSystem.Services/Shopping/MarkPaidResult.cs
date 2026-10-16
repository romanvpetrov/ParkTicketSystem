namespace TicketSystem.Services.Shopping;

public sealed record MarkPaidResult(
    bool Success,
    IReadOnlyList<TicketDto> Tickets,
    string? FailureCode,
    string? Message)
{
    public static MarkPaidResult Succeeded(IReadOnlyList<TicketDto> tickets)
    {
        return new MarkPaidResult(true, tickets, null, null);
    }

    public static MarkPaidResult Failed(string failureCode, string message)
    {
        return new MarkPaidResult(false, [], failureCode, message);
    }
}
