using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;
using TicketSystem.Services.Parks;

namespace TicketSystem.Tests.Parks;

public class ParkServiceTests
{
    [Fact]
    public void GetMonthlyAvailability_ReturnsPricesAndAvailableTicketsForEachScheduledDay()
    {
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(
            [
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 7, 1), 9_749, 749, 79.99m),
                new DailyInventory("adventurebay-food", "adventurebay", "Food Pass", new DateOnly(2026, 7, 1), 500, 100, 29.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 7, 2), 75, 75, 89.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", new DateOnly(2026, 8, 1), 30, 5, 69.99m)
            ]));

        var service = new ParkService(new StubParkRepository(park));

        var days = service.GetMonthlyAvailability("adventurebay", 2026, 7);

        var first = days[0];
        var second = days[1];
        var third = days[2];

        Assert.Equal("adventurebay-entry", first.ProductId);
        Assert.Equal("Park Ticket", first.ProductName);
        Assert.Equal(new DateOnly(2026, 7, 1), first.Date);
        Assert.Equal(9_000, first.AvailableTickets);
        Assert.Equal(79.99m, first.Price);

        Assert.Equal("adventurebay-food", second.ProductId);
        Assert.Equal("Food Pass", second.ProductName);
        Assert.Equal(new DateOnly(2026, 7, 1), second.Date);
        Assert.Equal(400, second.AvailableTickets);
        Assert.Equal(29.99m, second.Price);

        Assert.Equal("adventurebay-entry", third.ProductId);
        Assert.Equal("Park Ticket", third.ProductName);
        Assert.Equal(new DateOnly(2026, 7, 2), third.Date);
        Assert.Equal(0, third.AvailableTickets);
        Assert.Equal(89.99m, third.Price);
    }

    private sealed class StubParkRepository(Park park) : IParkRepository
    {
        public Park? GetByUniqueName(string uniqueName)
        {
            return park.UniqueName == uniqueName ? park : null;
        }
    }
}
