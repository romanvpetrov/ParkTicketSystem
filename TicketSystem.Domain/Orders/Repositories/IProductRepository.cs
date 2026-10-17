using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Repositories;

public interface IProductRepository
{
    IReadOnlyList<TicketProduct> GetByParkUniqueName(string parkUniqueName);
}
