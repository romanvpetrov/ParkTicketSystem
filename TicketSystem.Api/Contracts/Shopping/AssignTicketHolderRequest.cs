namespace TicketSystem.Api.Contracts.Shopping;

public sealed record AssignTicketHolderRequest(
    string FirstName,
    string LastName,
    string Email);
