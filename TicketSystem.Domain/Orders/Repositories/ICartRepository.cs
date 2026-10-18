using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Repositories;

public interface ICartRepository
{
    Cart? GetById(Guid cartId);

    void Save(Cart cart);

    void Commit();
}
