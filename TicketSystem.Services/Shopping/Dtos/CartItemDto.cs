namespace TicketSystem.Services.Shopping;

public sealed record CartItemDto(
    Guid Id,
    string ProductId,
    string ParkUniqueName,
    DateOnly VisitDate,
    int Quantity,
    decimal Price,
    decimal LineTotal,
    string? TicketHolderFirstName = null,
    string? TicketHolderLastName = null,
    string? TicketHolderEmail = null);
