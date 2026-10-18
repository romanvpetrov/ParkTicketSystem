using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Parks;

namespace TicketSystem.Tests.Parks;

public class ParkTests
{
    [Fact]
    public void Constructor_RejectsInventoryBelongingToAnotherPark()
    {
        var calendar = new ParkCalendar(
        [
            new DailyInventory(
                "otherpark-entry",
                "otherpark",
                "Other Park Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                49.99m)
        ]);

        Assert.Throws<ArgumentException>(() => new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            calendar));
    }

    [Fact]
    public void TryCreateCartItem_CreatesPricedItemFromDailyInventory()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var park = CreatePark(visitDate, totalItemCount: 10, soldItemCount: 2);

        var result = park.TryCreateCartItem("adventurebay-entry", visitDate, 3);

        Assert.True(result.Success);
        Assert.Equal(CartItemCreationFailure.None, result.Failure);
        Assert.NotNull(result.CartItem);
        Assert.Equal("adventurebay-entry", result.CartItem.ProductId);
        Assert.Equal("adventurebay", result.CartItem.ParkUniqueName);
        Assert.Equal(visitDate, result.CartItem.VisitDate);
        Assert.Equal(3, result.CartItem.Quantity);
        Assert.Equal(79.99m, result.CartItem.Price);
    }

    [Fact]
    public void TryCreateCartItem_ReturnsAvailableCountWhenInventoryIsInsufficient()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var park = CreatePark(visitDate, totalItemCount: 5, soldItemCount: 4);

        var result = park.TryCreateCartItem("adventurebay-entry", visitDate, 2);

        Assert.False(result.Success);
        Assert.Null(result.CartItem);
        Assert.Equal(CartItemCreationFailure.NotEnoughInventory, result.Failure);
        Assert.Equal(1, result.AvailableItemCount);
    }

    [Fact]
    public void TryCreateCartItem_ReturnsFailureWhenQuantityIsNotPositive()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var park = CreatePark(visitDate, totalItemCount: 5, soldItemCount: 0);

        var result = park.TryCreateCartItem("adventurebay-entry", visitDate, 0);

        Assert.False(result.Success);
        Assert.Null(result.CartItem);
        Assert.Equal(CartItemCreationFailure.InvalidQuantity, result.Failure);
    }

    [Fact]
    public void TryRecordItemsSold_UpdatesInventoryWhenItemsAreAvailable()
    {
        var inventory = new DailyInventory(
            "adventurebay-entry",
            "adventurebay",
            "Adventure Bay Entry",
            new DateOnly(2026, 7, 1),
            5,
            1,
            79.99m);

        var result = inventory.TryRecordItemsSold(2);

        Assert.True(result.Success);
        Assert.Equal(3, inventory.SoldItemCount);
        Assert.Equal(2, inventory.AvailableItemCount);
    }

    [Fact]
    public void TryRecordItemsSold_ReturnsFailureWhenInventoryIsInsufficient()
    {
        var inventory = new DailyInventory(
            "adventurebay-entry",
            "adventurebay",
            "Adventure Bay Entry",
            new DateOnly(2026, 7, 1),
            5,
            4,
            79.99m);

        var result = inventory.TryRecordItemsSold(2);

        Assert.False(result.Success);
        Assert.Equal(RecordItemsSoldFailure.NotEnoughInventory, result.Failure);
        Assert.Equal(1, result.AvailableItemCount);
        Assert.Equal(4, inventory.SoldItemCount);
    }

    private static Park CreatePark(DateOnly visitDate, int totalItemCount, int soldItemCount)
    {
        return new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(
            [
                new DailyInventory(
                    "adventurebay-entry",
                    "adventurebay",
                    "Adventure Bay Entry",
                    visitDate,
                    totalItemCount,
                    soldItemCount,
                    79.99m)
            ]));
    }
}
