using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;
using TicketSystem.Services.Parks;

namespace TicketSystem.Tests.Parks;

public class ParkServiceTests
{
    [Fact]
    public void GetParks_ReturnsParkSummaries()
    {
        var adventureBay = CreatePark(1, "adventurebay", "Adventure Bay");
        var splashHarbor = CreatePark(2, "splashharbor", "Splash Harbor");
        var service = new ParkService(new StubParkRepository(adventureBay, splashHarbor));

        var parks = service.GetParks();

        Assert.Equal(2, parks.Count);
        Assert.Equal("adventurebay", parks[0].UniqueName);
        Assert.Equal("Adventure Bay", parks[0].Name);
        Assert.Equal("Orlando, Florida", parks[0].Location);
        Assert.Equal("Single-day admission, food passes, and dated add-ons.", parks[0].TagText);
        Assert.Equal("America/New_York", parks[0].TimeZoneId);
        Assert.Equal("splashharbor", parks[1].UniqueName);
        Assert.Equal("Splash Harbor", parks[1].Name);
        Assert.Equal("Tampa, Florida", parks[1].Location);
        Assert.Equal("Water park admission, food passes, and dated add-ons.", parks[1].TagText);
        Assert.Equal("America/New_York", parks[1].TimeZoneId);
    }

    [Fact]
    public void GetParks_AllowsMissingPresentationMetadata()
    {
        var park = new Park(1, "adventurebay", "Adventure Bay", new ParkCalendar([]));
        var service = CreateParkService(park);

        var result = Assert.Single(service.GetParks());

        Assert.Null(result.Location);
        Assert.Null(result.TagText);
        Assert.Equal("America/New_York", result.TimeZoneId);
    }

    [Fact]
    public void GetMonthlyAvailability_ReturnsPricesAndAvailableTicketsForEachScheduledDay()
    {
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            "Orlando, Florida",
            "Single-day admission, food passes, and dated add-ons.",
            new ParkCalendar(
            [
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 1), 9_749, 749, 79.99m),
                new DailyInventory("adventurebay-plus", "adventurebay", "Adventure Bay Plus", new DateOnly(2026, 7, 1), 500, 100, 99.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 2), 75, 75, 89.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 8, 1), 30, 5, 69.99m)
            ]));

        var service = CreateParkService(park);

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
            "Orlando, Florida",
            "Single-day admission, food passes, and dated add-ons.",
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
        var service = CreateParkService(park);

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
            "Orlando, Florida",
            "Single-day admission, food passes, and dated add-ons.",
            new ParkCalendar(
                [
                    new TicketProduct("adventurebay-entry", "adventurebay", "Adventure Bay Entry", 100, 79.99m),
                    new TicketProduct("adventurebay-plus", "adventurebay", "Adventure Bay Plus", 50, 99.99m)
                ],
                []));
        var service = CreateParkService(park);

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
            "Orlando, Florida",
            "Single-day admission, food passes, and dated add-ons.",
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
        var service = CreateParkService(park);

        var availability = service.GetMonthlyAvailability("adventurebay", 2026, 7);
        var entry = Assert.Single(availability);

        Assert.Equal("adventurebay-entry", entry.ProductId);
        Assert.Equal("Adventure Bay Entry", entry.ProductName);
        Assert.Equal(visitDate, entry.Date);
        Assert.Equal(8, entry.AvailableTickets);
        Assert.Equal(89.99m, entry.Price);
    }

    [Fact]
    public void GetMonthlyAvailability_HidesDatesBeforeCurrentDateInParkTimeZone()
    {
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            "Orlando, Florida",
            null,
            "America/Los_Angeles",
            new ParkCalendar(
            [
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 1), 100, 0, 79.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 2), 100, 0, 79.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 3), 100, 0, 79.99m)
            ]));
        var service = new ParkService(
            new StubParkRepository(park),
            new StubTimeProvider(new DateTimeOffset(2026, 7, 3, 6, 0, 0, TimeSpan.Zero)));

        var availability = service.GetMonthlyAvailability("adventurebay", 2026, 7);

        Assert.Equal(2, availability.Count);
        Assert.Equal(new DateOnly(2026, 7, 2), availability[0].Date);
        Assert.Equal(new DateOnly(2026, 7, 3), availability[1].Date);
    }

    [Fact]
    public void GetMonthlyAvailability_DefaultsToFloridaTimeZone()
    {
        var park = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            "Orlando, Florida",
            null,
            null,
            new ParkCalendar(
            [
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 1), 100, 0, 79.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 2), 100, 0, 79.99m),
                new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", new DateOnly(2026, 7, 3), 100, 0, 79.99m)
            ]));
        var service = new ParkService(
            new StubParkRepository(park),
            new StubTimeProvider(new DateTimeOffset(2026, 7, 2, 5, 0, 0, TimeSpan.Zero)));

        var availability = service.GetMonthlyAvailability("adventurebay", 2026, 7);

        Assert.Equal(2, availability.Count);
        Assert.Equal(new DateOnly(2026, 7, 2), availability[0].Date);
        Assert.Equal(new DateOnly(2026, 7, 3), availability[1].Date);
    }

    private static Park CreatePark(int id, string uniqueName, string name)
    {
        var location = uniqueName == "adventurebay" ? "Orlando, Florida" : "Tampa, Florida";
        var tagText = uniqueName == "adventurebay"
            ? "Single-day admission, food passes, and dated add-ons."
            : "Water park admission, food passes, and dated add-ons.";

        return new Park(id, uniqueName, name, location, tagText, new ParkCalendar([]));
    }

    private static ParkService CreateParkService(Park park)
    {
        return new ParkService(
            new StubParkRepository(park),
            new StubTimeProvider(new DateTimeOffset(2026, 7, 1, 4, 0, 0, TimeSpan.Zero)));
    }

    private sealed class StubParkRepository(params Park[] parks) : IParkRepository
    {
        public IReadOnlyList<Park> GetAll()
        {
            return parks;
        }

        public Park? GetByUniqueName(string uniqueName)
        {
            return parks.SingleOrDefault(park => park.UniqueName == uniqueName);
        }

        public void Save(Park savedPark)
        {
        }

        public void Commit()
        {
        }
    }

    private sealed class StubTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
