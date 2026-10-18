using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Parks;

public sealed class Park
{
    public Park(int id, string uniqueName, string name, ParkCalendar calendar)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(uniqueName));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Park name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(calendar);

        if (calendar.DailyInventories.Any(inventory => inventory.ParkUniqueName != uniqueName) ||
            calendar.TicketProducts.Any(product => product.ParkUniqueName != uniqueName))
        {
            throw new ArgumentException("The park calendar contains products or inventory for another park.", nameof(calendar));
        }

        Id = id;
        UniqueName = uniqueName;
        Name = name;
        Calendar = calendar;
    }

    public int Id { get; }

    public string UniqueName { get; }

    public string Name { get; }

    public ParkCalendar Calendar { get; }

    public IReadOnlyList<DailyInventory> GetMonthlyInventory(int year, int month)
    {
        return Calendar.GetMonthlyInventory(year, month);
    }

    public DailyInventory? GetDailyInventory(string productId, DateOnly date)
    {
        return Calendar.GetDailyInventory(productId, date);
    }

    public CartItemCreationResult TryCreateCartItem(string productId, DateOnly date, int quantity)
    {
        var inventory = Calendar.GetDailyInventory(productId, date);

        return inventory is null
            ? CartItemCreationResult.Failed(CartItemCreationFailure.ProductNotFound)
            : inventory.TryCreateCartItem(quantity);
    }

    public RecordTicketsSoldResult RecordTicketsSold(IReadOnlyList<Ticket> tickets)
    {
        var ticketsForPark = tickets
            .Where(ticket => ticket.ParkUniqueName == UniqueName)
            .ToArray();

        return Calendar.RecordTicketsSold(ticketsForPark);
    }
}
