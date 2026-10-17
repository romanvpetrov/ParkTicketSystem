using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Parks;

public sealed class ParkCalendar
{
    private readonly IReadOnlyList<DailyInventory> _dailyInventories;

    /// <summary>
    /// Creates a calendar with inventory ordered by visit date so callers get stable results.
    /// </summary>
    public ParkCalendar(IEnumerable<DailyInventory> dailyInventories)
    {
        _dailyInventories = dailyInventories
            .OrderBy(dailyInventory => dailyInventory.Date)
            .ToArray();
    }

    public IReadOnlyList<DailyInventory> DailyInventories => _dailyInventories;

    /// <summary>
    /// Returns every product available on dates in the requested month.
    /// </summary>
    public IReadOnlyList<DailyInventory> GetMonthlyInventory(int year, int month)
    {
        return _dailyInventories
            .Where(dailyInventory => dailyInventory.Date.Year == year && dailyInventory.Date.Month == month)
            .ToArray();
    }

    /// <summary>
    /// Finds the inventory record for a specific product on a specific visit date.
    /// </summary>
    public DailyInventory? GetDailyInventory(string productId, DateOnly date)
    {
        return _dailyInventories
            .SingleOrDefault(dailyInventory =>
                dailyInventory.ProductId == productId &&
                dailyInventory.Date == date);
    }

    public void RecordTicketsSold(IReadOnlyList<Ticket> tickets)
    {
        var sales = tickets
            .GroupBy(ticket => new { ticket.ProductId, ticket.VisitDate })
            .Select(group => new
            {
                Inventory = GetDailyInventory(group.Key.ProductId, group.Key.VisitDate)
                    ?? throw new InvalidOperationException(
                        $"Product '{group.Key.ProductId}' was not found for {group.Key.VisitDate}."),
                Quantity = group.Count()
            })
            .ToArray();

        foreach (var sale in sales)
        {
            sale.Inventory.RecordItemsSold(sale.Quantity);
        }
    }
}
