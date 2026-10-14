namespace TicketSystem.Services.Shopping;

public sealed record AssignTicketHolderResult(
    bool Success,
    string? FailureCode,
    string? Message)
{
    public static AssignTicketHolderResult Succeeded()
    {
        return new AssignTicketHolderResult(true, null, null);
    }

    public static AssignTicketHolderResult Failed(string failureCode, string message)
    {
        return new AssignTicketHolderResult(false, failureCode, message);
    }
}
