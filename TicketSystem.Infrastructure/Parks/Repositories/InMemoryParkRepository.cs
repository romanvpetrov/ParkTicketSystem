using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Parks;

public sealed class InMemoryParkRepository : IParkRepository
{
    private readonly List<Park> _parks =
    [
        new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(
            [
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 7, 1), 50, 20, 79.99m),
                new DailyInventory("adventurebay-food", "adventurebay", "Food Pass", new DateOnly(2026, 7, 1), 25, 5, 29.99m),
                new DailyInventory("adventurebay-photos", "adventurebay", "Photo Pass", new DateOnly(2026, 7, 1), 25, 5, 29.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 7, 2), 75, 75, 89.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 7, 3), 100, 12, 84.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 8, 1), 30, 5, 69.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 10, 1), 30, 5, 69.99m)
            ]))
    ];

    public Park? GetByUniqueName(string uniqueName)
    {
        return _parks.SingleOrDefault(park => park.UniqueName == uniqueName);
    }

    public void Save(Park park)
    {
        var existingPark = _parks.SingleOrDefault(existing => existing.UniqueName == park.UniqueName);

        if (existingPark is not null)
        {
            _parks.Remove(existingPark);
        }

        _parks.Add(park);
    }
}
