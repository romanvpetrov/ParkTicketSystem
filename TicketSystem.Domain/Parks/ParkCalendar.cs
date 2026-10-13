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
}
