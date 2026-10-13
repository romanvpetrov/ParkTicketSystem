using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class InMemoryCartRepository : ICartRepository
{
    private readonly Dictionary<Guid, Cart> _carts = [];

    public Cart? GetById(Guid cartId)
    {
        return _carts.GetValueOrDefault(cartId);
    }

    public void Save(Cart cart)
    {
        _carts[cart.Id] = cart;
    }
}
