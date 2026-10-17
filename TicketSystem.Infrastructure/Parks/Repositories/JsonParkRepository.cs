using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;
using TicketSystem.Infrastructure.Persistence;

namespace TicketSystem.Infrastructure.Parks;

public sealed class JsonParkRepository : IParkRepository
{
    private readonly IProductRepository _productRepository;
    private readonly JsonFileStore<List<ParkDocument>> _store;

    public JsonParkRepository(string dataDirectory, IProductRepository productRepository)
    {
        _productRepository = productRepository;
        _store = new JsonFileStore<List<ParkDocument>>(dataDirectory, "parks.json", CreateInitialDocuments);
    }

    public Park? GetByUniqueName(string uniqueName)
    {
        return _store.Read(parks =>
        {
            var document = parks.SingleOrDefault(park => park.UniqueName == uniqueName);
            return document is null ? null : ToDomain(document);
        });
    }

    public void Save(Park park)
    {
        _store.Update(parks =>
        {
            parks.RemoveAll(existing => existing.UniqueName == park.UniqueName);
            parks.Add(ToDocument(park));
        });
    }

    private static List<ParkDocument> CreateInitialDocuments()
    {
        return
        [
            new ParkDocument(
                1,
                "adventurebay",
                "Adventure Bay",
                [
                    new DailyInventoryDocument("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 1), 50, 20, 79.99m),
                    new DailyInventoryDocument("adventurebay-plus", "adventurebay", "Adventure Bay Plus", new DateOnly(2026, 7, 1), 25, 5, 99.99m),
                    new DailyInventoryDocument("adventurebay-premium", "adventurebay", "Adventure Bay Premium", new DateOnly(2026, 7, 1), 25, 5, 129.99m),
                    new DailyInventoryDocument("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 2), 75, 75, 89.99m),
                    new DailyInventoryDocument("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 3), 100, 12, 84.99m),
                    new DailyInventoryDocument("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 8, 1), 30, 5, 69.99m),
                    new DailyInventoryDocument("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 10, 1), 30, 5, 69.99m)
                ])
        ];
    }

    private Park ToDomain(ParkDocument park)
    {
        return new Park(
            park.Id,
            park.UniqueName,
            park.Name,
            new ParkCalendar(
                _productRepository.GetByParkUniqueName(park.UniqueName),
                park.DailyInventories.Select(inventory => new DailyInventory(
                    inventory.ProductId,
                    inventory.ParkUniqueName,
                    inventory.Name,
                    inventory.Date,
                    inventory.TotalItemCount,
                    inventory.SoldItemCount,
                    inventory.Price))));
    }

    private static ParkDocument ToDocument(Park park)
    {
        return new ParkDocument(
            park.Id,
            park.UniqueName,
            park.Name,
            park.Calendar.DailyInventories.Select(inventory => new DailyInventoryDocument(
                inventory.ProductId,
                inventory.ParkUniqueName,
                inventory.Name,
                inventory.Date,
                inventory.TotalItemCount,
                inventory.SoldItemCount,
                inventory.Price)).ToList());
    }

    private sealed record ParkDocument(
        int Id,
        string UniqueName,
        string Name,
        List<DailyInventoryDocument> DailyInventories);

    private sealed record DailyInventoryDocument(
        string ProductId,
        string ParkUniqueName,
        string Name,
        DateOnly Date,
        int TotalItemCount,
        int SoldItemCount,
        decimal Price);
}
