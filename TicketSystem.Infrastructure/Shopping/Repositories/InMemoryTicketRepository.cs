using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly Dictionary<Guid, Ticket> _tickets = [];

    public Ticket? GetById(Guid ticketId)
    {
        return _tickets.GetValueOrDefault(ticketId);
    }

    public void Save(Ticket ticket)
    {
        _tickets[ticket.Id] = ticket;
    }
}
