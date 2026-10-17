namespace TicketSystem.Services.Shopping;

public sealed record AddToCartResult(
    bool Success,
    Guid CartId,
    string? FailureCode,
    string? Message)
{
    public static AddToCartResult Succeeded(Guid cartId)
    {
        return new AddToCartResult(true, cartId, null, null);
    }

    public static AddToCartResult Failed(string failureCode, string message)
    {
        return new AddToCartResult(false, Guid.Empty, failureCode, message);
    }
}
