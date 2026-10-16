using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Repositories;

public interface ITicketRepository
{
    Ticket? GetById(Guid ticketId);

    void Save(Ticket ticket);
}
