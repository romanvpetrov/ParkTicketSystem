namespace TicketSystem.Services.Shopping;

public sealed record RemoveFromCartResult(
    bool Success,
    string? FailureCode,
    string? Message)
{
    public static RemoveFromCartResult Succeeded()
    {
        return new RemoveFromCartResult(true, null, null);
    }

    public static RemoveFromCartResult Failed(string failureCode, string message)
    {
        return new RemoveFromCartResult(false, failureCode, message);
    }
}
