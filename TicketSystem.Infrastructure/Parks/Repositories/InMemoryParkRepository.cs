using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Infrastructure.Parks;

public sealed class InMemoryParkRepository : IParkRepository
{
    private readonly List<Park> _parks;
    private readonly List<Park> _stagedParks = [];

    public InMemoryParkRepository(IProductRepository productRepository)
    {
        _parks =
        [
            new Park(
                1,
                "adventurebay",
                "Adventure Bay",
                "Orlando, Florida",
                null,
                "America/New_York",
                new ParkCalendar(
                productRepository.GetByParkUniqueName("adventurebay"),
                [
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 1), 50, 20, 79.99m),
                new DailyInventory("adventurebay-plus", "adventurebay", "Adventure Bay Plus", new DateOnly(2026, 7, 1), 25, 5, 99.99m),
                new DailyInventory("adventurebay-premium", "adventurebay", "Adventure Bay Premium", new DateOnly(2026, 7, 1), 25, 5, 129.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 2), 75, 75, 89.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 3), 100, 12, 84.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 8, 1), 30, 5, 69.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 10, 1), 30, 5, 69.99m)
                ])),
            new Park(
                2,
                "splashharbor",
                "Splash Harbor",
                "Tampa, Florida",
                null,
                "America/New_York",
                new ParkCalendar(
                productRepository.GetByParkUniqueName("splashharbor"),
                [
                new DailyInventory("splashharbor-entry", "splashharbor", "Splash Harbor Entry", new DateOnly(2026, 7, 1), 40, 8, 59.99m),
                new DailyInventory("splashharbor-food-pass", "splashharbor", "Splash Harbor Food Pass", new DateOnly(2026, 7, 1), 100, 15, 24.99m),
                new DailyInventory("splashharbor-entry", "splashharbor", "Splash Harbor Entry", new DateOnly(2026, 7, 2), 60, 0, 64.99m)
                ]))
        ];
    }

    public IReadOnlyList<Park> GetAll()
    {
        return _parks.ToArray();
    }

    public Park? GetByUniqueName(string uniqueName)
    {
        return _parks.SingleOrDefault(park => park.UniqueName == uniqueName);
    }

    public void Save(Park park)
    {
        var existingPark = _stagedParks.SingleOrDefault(existing => existing.UniqueName == park.UniqueName);

        if (existingPark is not null)
        {
            _stagedParks.Remove(existingPark);
        }

        _stagedParks.Add(park);
    }

    public void Commit()
    {
        foreach (var park in _stagedParks)
        {
            var existingPark = _parks.SingleOrDefault(existing => existing.UniqueName == park.UniqueName);

            if (existingPark is not null)
            {
                _parks.Remove(existingPark);
            }

            _parks.Add(park);
        }

        _stagedParks.Clear();
    }
}
