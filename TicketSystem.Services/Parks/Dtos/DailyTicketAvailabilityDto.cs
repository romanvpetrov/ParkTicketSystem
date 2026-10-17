namespace TicketSystem.Services.Parks;

public sealed record DailyTicketAvailabilityDto(
    string ProductId,
    string ProductName,
    DateOnly Date,
    int AvailableTickets,
    decimal Price);
