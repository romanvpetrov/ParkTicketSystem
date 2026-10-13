namespace TicketSystem.Services.Shopping;

public sealed record CartItemDto(
    string ProductId,
    string ParkUniqueName,
    DateOnly VisitDate,
    int Quantity,
    decimal Price,
    decimal LineTotal);
