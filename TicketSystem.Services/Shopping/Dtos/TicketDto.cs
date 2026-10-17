namespace TicketSystem.Services.Shopping;

public sealed record TicketDto(
    Guid Id,
    Guid CartItemId,
    string ProductId,
    string ParkUniqueName,
    DateOnly VisitDate,
    decimal Price,
    string FirstName,
    string LastName,
    string Email);
