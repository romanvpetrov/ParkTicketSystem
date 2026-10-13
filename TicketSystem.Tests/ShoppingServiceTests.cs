namespace TicketSystem.Tests;

using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;
using TicketSystem.Services.Shopping;

public class ShoppingServiceTests
{
    [Fact]
    public void AddToCart_ReturnsCartIdAndAddsSelectedTicketsToTheCart()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart("adventurebay-entry", 3, 2026, 7, 1);

        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.CartId);
        Assert.Null(result.FailureCode);
        Assert.Null(result.Message);

        var cartItems = shoppingService.GetCartItems(result.CartId);
        var cartItem = Assert.Single(cartItems);

        Assert.Equal("adventurebay-entry", cartItem.ProductId);
        Assert.Equal("adventurebay", cartItem.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), cartItem.VisitDate);
        Assert.Equal(3, cartItem.Quantity);
        Assert.Equal(79.99m, cartItem.Price);
        Assert.Equal(239.97m, cartItem.LineTotal);
    }

    [Fact]
    public void GetCartItems_ReturnsItemsCurrentlyInTheCart()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m),
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 2),
                10,
                0,
                89.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);

        var firstResult = shoppingService.AddToCart("adventurebay-entry", 2, 2026, 7, 1);
        shoppingService.AddToCart(firstResult.CartId, "adventurebay-entry", 1, 2026, 7, 2);

        var cartItems = shoppingService.GetCartItems(firstResult.CartId);

        Assert.Equal(2, cartItems.Count);

        var first = cartItems[0];
        var second = cartItems[1];

        Assert.Equal("adventurebay-entry", first.ProductId);
        Assert.Equal("adventurebay", first.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), first.VisitDate);
        Assert.Equal(2, first.Quantity);
        Assert.Equal(79.99m, first.Price);
        Assert.Equal(159.98m, first.LineTotal);

        Assert.Equal("adventurebay-entry", second.ProductId);
        Assert.Equal("adventurebay", second.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 2), second.VisitDate);
        Assert.Equal(1, second.Quantity);
        Assert.Equal(89.99m, second.Price);
        Assert.Equal(89.99m, second.LineTotal);
    }

    [Fact]
    public void AddToCart_CombinesQuantityWhenAddingSameProductTwice()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);

        var firstResult = shoppingService.AddToCart("adventurebay-entry", 2, 2026, 7, 1);
        shoppingService.AddToCart(firstResult.CartId, "adventurebay-entry", 3, 2026, 7, 1);

        var cartItems = shoppingService.GetCartItems(firstResult.CartId);
        var cartItem = Assert.Single(cartItems);

        Assert.Equal("adventurebay-entry", cartItem.ProductId);
        Assert.Equal("adventurebay", cartItem.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), cartItem.VisitDate);
        Assert.Equal(5, cartItem.Quantity);
        Assert.Equal(79.99m, cartItem.Price);
        Assert.Equal(399.95m, cartItem.LineTotal);
    }

    [Fact]
    public void AddToCart_ReturnsNewCartIdWhenProvidedCartDoesNotExist()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);
        var missingCartId = Guid.NewGuid();

        var result = shoppingService.AddToCart(missingCartId, "adventurebay-entry", 2, 2026, 7, 1);

        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.CartId);
        Assert.NotEqual(missingCartId, result.CartId);
        Assert.Null(result.FailureCode);
        Assert.Null(result.Message);

        var cartItems = shoppingService.GetCartItems(result.CartId);
        var cartItem = Assert.Single(cartItems);

        Assert.Equal("adventurebay-entry", cartItem.ProductId);
        Assert.Equal("adventurebay", cartItem.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), cartItem.VisitDate);
        Assert.Equal(2, cartItem.Quantity);
        Assert.Equal(79.99m, cartItem.Price);
        Assert.Equal(159.98m, cartItem.LineTotal);
    }

    [Fact]
    public void AddToCart_ReturnsFailureWhenQuantityIsGreaterThanAvailableItems()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-food",
                "adventurebay",
                "Food Pass",
                new DateOnly(2026, 7, 1),
                5,
                4,
                29.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart("adventurebay-food", 2, 2026, 7, 1);

        Assert.False(result.Success);
        Assert.Equal(Guid.Empty, result.CartId);
        Assert.Equal("NotEnoughInventory", result.FailureCode);
        Assert.Equal("Only 1 item is available for adventurebay-food.", result.Message);
        Assert.Empty(shoppingService.GetCartItems(result.CartId));
    }

    [Fact]
    public void RemoveFromCart_RemovesSelectedItemFromTheCart()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay-entry", 3, 2026, 7, 1);

        var removeResult = shoppingService.RemoveFromCart(addResult.CartId, "adventurebay-entry", 2026, 7, 1);

        Assert.True(removeResult.Success);
        Assert.Null(removeResult.FailureCode);
        Assert.Null(removeResult.Message);
        Assert.Empty(shoppingService.GetCartItems(addResult.CartId));
    }

    [Fact]
    public void RemoveFromCart_RemovesSelectedItemAndLeavesOtherItemsInTheCart()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m),
            new DailyInventory(
                "adventurebay-food",
                "adventurebay",
                "Food Pass",
                new DateOnly(2026, 7, 1),
                10,
                0,
                29.99m));
        var shoppingService = new ShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay-entry", 2, 2026, 7, 1);
        shoppingService.AddToCart(addResult.CartId, "adventurebay-food", 1, 2026, 7, 1);

        var removeResult = shoppingService.RemoveFromCart(addResult.CartId, "adventurebay-entry", 2026, 7, 1);

        Assert.True(removeResult.Success);
        Assert.Null(removeResult.FailureCode);
        Assert.Null(removeResult.Message);

        var cartItems = shoppingService.GetCartItems(addResult.CartId);
        var remainingItem = Assert.Single(cartItems);

        Assert.Equal("adventurebay-food", remainingItem.ProductId);
        Assert.Equal("adventurebay", remainingItem.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), remainingItem.VisitDate);
        Assert.Equal(1, remainingItem.Quantity);
        Assert.Equal(29.99m, remainingItem.Price);
        Assert.Equal(29.99m, remainingItem.LineTotal);
    }

    private sealed class StubCartRepository : ICartRepository
    {
        private readonly Dictionary<Guid, Cart> _carts = [];

        public Cart? GetById(Guid cartId)
        {
            return _carts.GetValueOrDefault(cartId);
        }

        public void Save(Cart cart) => _carts[cart.Id] = cart;
    }

    private sealed class StubParkRepository(params DailyInventory[] products) : IParkRepository
    {
        private readonly Park _park = new(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(products));

        public Park? GetByUniqueName(string uniqueName)
        {
            return _park.UniqueName == uniqueName ? _park : null;
        }
    }
}
