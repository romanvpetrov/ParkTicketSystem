using TicketSystem.Domain.Orders;
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
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 1), 9_749, 749, 79.99m),
                new DailyInventory("adventurebay-plus", "adventurebay", "Adventure Bay Plus", new DateOnly(2026, 7, 1), 500, 100, 99.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 2), 75, 75, 89.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 8, 1), 30, 5, 69.99m)
            ]));

        var service = new ParkService(new StubParkRepository(park));

        var days = service.GetMonthlyAvailability("adventurebay", 2026, 7);

        var first = days[0];
        var second = days[1];
        var third = days[2];

        Assert.Equal("adventurebay-entry", first.ProductId);
        Assert.Equal("Adventure Bay Entry", first.ProductName);
        Assert.Equal(new DateOnly(2026, 7, 1), first.Date);
        Assert.Equal(9_000, first.AvailableTickets);
        Assert.Equal(79.99m, first.Price);

        Assert.Equal("adventurebay-plus", second.ProductId);
        Assert.Equal("Adventure Bay Plus", second.ProductName);
        Assert.Equal(new DateOnly(2026, 7, 1), second.Date);
        Assert.Equal(400, second.AvailableTickets);
        Assert.Equal(99.99m, second.Price);

        Assert.Equal("adventurebay-entry", third.ProductId);
        Assert.Equal("Adventure Bay Entry", third.ProductName);
        Assert.Equal(new DateOnly(2026, 7, 2), third.Date);
        Assert.Equal(0, third.AvailableTickets);
        Assert.Equal(89.99m, third.Price);
    }

    [Fact]
    public void GetMonthlyAvailability_UsesDailyInventoryForOneProductAndDefaultsForAnother()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(
                [
                    new TicketProduct("adventurebay-entry", "adventurebay", "Adventure Bay Entry", 100, 79.99m),
                    new TicketProduct("adventurebay-plus", "adventurebay", "Adventure Bay Plus", 50, 99.99m)
                ],
                [
                    new DailyInventory(
                        "adventurebay-entry",
                        "adventurebay",
                        "Adventure Bay Entry",
                        visitDate,
                        10,
                        2,
                        89.99m)
                ]));
        var service = new ParkService(new StubParkRepository(park));

        var availability = service.GetMonthlyAvailability("adventurebay", 2026, 7);
        var entry = Assert.Single(
            availability,
            item => item.ProductId == "adventurebay-entry" && item.Date == visitDate);
        var plus = Assert.Single(
            availability,
            item => item.ProductId == "adventurebay-plus" && item.Date == visitDate);

        Assert.Equal(8, entry.AvailableTickets);
        Assert.Equal(89.99m, entry.Price);
        Assert.Equal(50, plus.AvailableTickets);
        Assert.Equal(99.99m, plus.Price);
    }

    [Fact]
    public void GetMonthlyAvailability_UsesTicketProductDefaultsWhenNoDailyInventoryExists()
    {
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(
                [
                    new TicketProduct("adventurebay-entry", "adventurebay", "Adventure Bay Entry", 100, 79.99m),
                    new TicketProduct("adventurebay-plus", "adventurebay", "Adventure Bay Plus", 50, 99.99m)
                ],
                []));
        var service = new ParkService(new StubParkRepository(park));

        var availability = service.GetMonthlyAvailability("adventurebay", 2026, 7)
            .Where(item => item.Date.Day <= 3)
            .ToArray();

        Assert.Equal(6, availability.Length);

        Assert.Equal("adventurebay-entry", availability[0].ProductId);
        Assert.Equal(new DateOnly(2026, 7, 1), availability[0].Date);
        Assert.Equal(100, availability[0].AvailableTickets);
        Assert.Equal(79.99m, availability[0].Price);

        Assert.Equal("adventurebay-plus", availability[1].ProductId);
        Assert.Equal(new DateOnly(2026, 7, 1), availability[1].Date);
        Assert.Equal(50, availability[1].AvailableTickets);
        Assert.Equal(99.99m, availability[1].Price);

        Assert.Equal("adventurebay-entry", availability[2].ProductId);
        Assert.Equal(new DateOnly(2026, 7, 2), availability[2].Date);
        Assert.Equal("adventurebay-plus", availability[3].ProductId);
        Assert.Equal(new DateOnly(2026, 7, 2), availability[3].Date);

        Assert.Equal("adventurebay-entry", availability[4].ProductId);
        Assert.Equal(new DateOnly(2026, 7, 3), availability[4].Date);
        Assert.Equal("adventurebay-plus", availability[5].ProductId);
        Assert.Equal(new DateOnly(2026, 7, 3), availability[5].Date);
    }

    [Fact]
    public void GetMonthlyAvailability_ReturnsDailyInventoryWhenNoTicketProductsExist()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(
                [],
                [
                    new DailyInventory(
                        "adventurebay-entry",
                        "adventurebay",
                        "Adventure Bay Entry",
                        visitDate,
                        10,
                        2,
                        89.99m)
                ]));
        var service = new ParkService(new StubParkRepository(park));

        var availability = service.GetMonthlyAvailability("adventurebay", 2026, 7);
        var entry = Assert.Single(availability);

        Assert.Equal("adventurebay-entry", entry.ProductId);
        Assert.Equal("Adventure Bay Entry", entry.ProductName);
        Assert.Equal(visitDate, entry.Date);
        Assert.Equal(8, entry.AvailableTickets);
        Assert.Equal(89.99m, entry.Price);
    }

    private sealed class StubParkRepository(Park park) : IParkRepository
    {
        public Park? GetByUniqueName(string uniqueName)
        {
            return park.UniqueName == uniqueName ? park : null;
        }

        public void Save(Park savedPark)
        {
        }
    }
}
