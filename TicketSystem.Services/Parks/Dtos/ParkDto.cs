namespace TicketSystem.Services.Parks;

public sealed record ParkDto(
    string UniqueName,
    string Name,
    string? Location,
    string? TagText,
    string TimeZoneId);
