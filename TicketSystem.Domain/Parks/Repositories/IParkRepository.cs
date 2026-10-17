using TicketSystem.Domain.Parks;

namespace TicketSystem.Domain.Repositories;

public interface IParkRepository
{
    Park? GetByUniqueName(string uniqueName);

    void Save(Park park);
}
