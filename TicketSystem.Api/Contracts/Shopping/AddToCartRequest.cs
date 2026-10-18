namespace TicketSystem.Api.Contracts.Shopping;

public sealed record AddToCartRequest(
    string ParkUniqueName,
    string ProductId,
    int Quantity,
    int Year,
    int Month,
    int Day);
