using TicketSystem.Domain.Parks;

namespace TicketSystem.Domain.Repositories;

public interface IParkRepository
{
    IReadOnlyList<Park> GetAll();

    Park? GetByUniqueName(string uniqueName);

    void Save(Park park);

    void Commit();
}
