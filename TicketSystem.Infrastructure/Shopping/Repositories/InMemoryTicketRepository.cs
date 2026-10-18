using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class InMemoryTicketRepository : ITicketRepository
{
    private readonly Dictionary<Guid, Ticket> _tickets = [];
    private readonly Dictionary<Guid, Ticket> _stagedTickets = [];

    public Ticket? GetById(Guid ticketId)
    {
        return _tickets.GetValueOrDefault(ticketId);
    }

    public void Save(Ticket ticket)
    {
        _stagedTickets[ticket.Id] = ticket;
    }

    public void Commit()
    {
        foreach (var ticket in _stagedTickets)
        {
            _tickets[ticket.Key] = ticket.Value;
        }

        _stagedTickets.Clear();
    }
}
