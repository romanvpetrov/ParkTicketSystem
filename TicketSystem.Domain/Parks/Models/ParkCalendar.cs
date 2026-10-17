using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Parks;

public sealed class ParkCalendar
{
    private readonly IReadOnlyList<TicketProduct> _ticketProducts;
    private readonly List<DailyInventory> _dailyInventories;

    /// <summary>
    /// Creates a calendar with inventory ordered by visit date so callers get stable results.
    /// </summary>
    public ParkCalendar(IEnumerable<DailyInventory> dailyInventories)
        : this([], dailyInventories)
    {
    }

    public ParkCalendar(
        IEnumerable<TicketProduct> ticketProducts,
        IEnumerable<DailyInventory> dailyInventories)
    {
        _ticketProducts = ticketProducts.ToArray();
        _dailyInventories = dailyInventories
            .OrderBy(dailyInventory => dailyInventory.Date)
            .ToList();
    }

    public IReadOnlyList<TicketProduct> TicketProducts => _ticketProducts;

    public IReadOnlyList<DailyInventory> DailyInventories => _dailyInventories;

    /// <summary>
    /// Returns every product available on dates in the requested month.
    /// </summary>
    public IReadOnlyList<DailyInventory> GetMonthlyInventory(int year, int month)
    {
        if (_ticketProducts.Count == 0)
        {
            return _dailyInventories
                .Where(dailyInventory => dailyInventory.Date.Year == year && dailyInventory.Date.Month == month)
                .ToArray();
        }

        var inventory = new List<DailyInventory>();
        var daysInMonth = DateTime.DaysInMonth(year, month);

        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);

            inventory.AddRange(_ticketProducts.Select(product => GetDailyInventory(product.Id, date)!));
            inventory.AddRange(_dailyInventories.Where(dailyInventory =>
                dailyInventory.Date == date &&
                _ticketProducts.All(product => product.Id != dailyInventory.ProductId)));
        }

        return inventory;
    }

    /// <summary>
    /// Finds the inventory record for a specific product on a specific visit date.
    /// </summary>
    public DailyInventory? GetDailyInventory(string productId, DateOnly date)
    {
        var dailyInventory = FindDailyInventory(productId, date);

        if (dailyInventory is not null)
        {
            return dailyInventory;
        }

        return _ticketProducts
            .SingleOrDefault(product => product.Id == productId)
            ?.CreateDailyInventory(date);
    }

    public void AddDailyInventory(DailyInventory dailyInventory)
    {
        if (FindDailyInventory(dailyInventory.ProductId, dailyInventory.Date) is not null)
        {
            throw new InvalidOperationException(
                $"Product '{dailyInventory.ProductId}' already has inventory for {dailyInventory.Date}.");
        }

        _dailyInventories.Add(dailyInventory);
        _dailyInventories.Sort((left, right) => left.Date.CompareTo(right.Date));
    }

    public void RecordTicketsSold(IReadOnlyList<Ticket> tickets)
    {
        var sales = tickets
            .GroupBy(ticket => new { ticket.ProductId, ticket.VisitDate })
            .Select(group => new
            {
                ExistingInventory = FindDailyInventory(group.Key.ProductId, group.Key.VisitDate),
                Inventory = GetDailyInventory(group.Key.ProductId, group.Key.VisitDate)
                    ?? throw new InvalidOperationException(
                        $"Product '{group.Key.ProductId}' was not found for {group.Key.VisitDate}."),
                Quantity = group.Count()
            })
            .ToArray();

        foreach (var sale in sales)
        {
            if (sale.ExistingInventory is null)
            {
                AddDailyInventory(sale.Inventory);
            }

            sale.Inventory.RecordItemsSold(sale.Quantity);
        }
    }

    private DailyInventory? FindDailyInventory(string productId, DateOnly date)
    {
        return _dailyInventories
            .SingleOrDefault(dailyInventory =>
                dailyInventory.ProductId == productId &&
                dailyInventory.Date == date);
    }
}
