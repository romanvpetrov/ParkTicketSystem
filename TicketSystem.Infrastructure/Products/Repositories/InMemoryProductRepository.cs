using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Products;

public sealed class InMemoryProductRepository : IProductRepository
{
    private readonly IReadOnlyList<TicketProduct> _products =
    [
        new TicketProduct("adventurebay-entry", "adventurebay", "Adventure Bay Entry", 50, 79.99m),
        new TicketProduct("adventurebay-plus", "adventurebay", "Adventure Bay Plus", 25, 99.99m),
        new TicketProduct("adventurebay-premium", "adventurebay", "Adventure Bay Premium", 25, 129.99m)
    ];

    public IReadOnlyList<TicketProduct> GetByParkUniqueName(string parkUniqueName)
    {
        return _products
            .Where(product => product.ParkUniqueName == parkUniqueName)
            .ToArray();
    }
}
