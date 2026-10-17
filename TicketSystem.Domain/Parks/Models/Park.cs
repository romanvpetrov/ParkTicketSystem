using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Parks;

public sealed class Park
{
    public Park(int id, string uniqueName, string name, ParkCalendar calendar)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Park name is required.", nameof(name));
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

    public DailyInventory? GetInventoryOrDefault(string productId, DateOnly date)
    {
        return Calendar.GetDailyInventory(productId, date);
    }

    public void RecordTicketsSold(IReadOnlyList<Ticket> tickets)
    {
        var ticketsForPark = tickets
            .Where(ticket => ticket.ParkUniqueName == UniqueName)
            .ToArray();

        Calendar.RecordTicketsSold(ticketsForPark);
    }
}
