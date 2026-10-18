using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class InMemoryCartRepository : ICartRepository
{
    private readonly Dictionary<Guid, Cart> _carts = [];
    private readonly Dictionary<Guid, Cart> _stagedCarts = [];

    public Cart? GetById(Guid cartId)
    {
        return _carts.GetValueOrDefault(cartId);
    }

    public void Save(Cart cart)
    {
        _stagedCarts[cart.Id] = cart;
    }

    public void Commit()
    {
        foreach (var cart in _stagedCarts)
        {
            _carts[cart.Key] = cart.Value;
        }

        _stagedCarts.Clear();
    }
}
