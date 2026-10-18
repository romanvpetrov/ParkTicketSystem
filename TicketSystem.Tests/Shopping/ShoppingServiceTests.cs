namespace TicketSystem.Tests.Shopping;

using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;
using TicketSystem.Services.Parks;
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
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 3, 2026, 7, 1);

        Assert.True(result.Success);
        Assert.NotEqual(Guid.Empty, result.CartId);
        Assert.Null(result.FailureCode);
        Assert.Null(result.Message);

        var cartItems = shoppingService.GetCartItems(result.CartId);
        var cartItem = Assert.Single(cartItems);

        Assert.NotEqual(Guid.Empty, cartItem.Id);
        Assert.Equal("adventurebay-entry", cartItem.ProductId);
        Assert.Equal("adventurebay", cartItem.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), cartItem.VisitDate);
        Assert.Equal(3, cartItem.Quantity);
        Assert.Equal(79.99m, cartItem.Price);
        Assert.Equal(239.97m, cartItem.LineTotal);
    }

    [Fact]
    public void AddToCart_LoadsTheRequestedPark()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new Park(
                2,
                "splashharbor",
                "Splash Harbor",
                "Tampa, Florida",
                "Water park admission, food passes, and dated add-ons.",
                new ParkCalendar(
                [
                    new DailyInventory(
                        "splashharbor-entry",
                        "splashharbor",
                        "Splash Harbor Entry",
                        new DateOnly(2026, 7, 1),
                        40,
                        8,
                        59.99m)
                ])));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart(
            "splashharbor",
            "splashharbor-entry",
            1,
            2026,
            7,
            1);

        Assert.True(result.Success);
        var cartItem = Assert.Single(shoppingService.GetCartItems(result.CartId));
        Assert.Equal("splashharbor-entry", cartItem.ProductId);
        Assert.Equal("splashharbor", cartItem.ParkUniqueName);
        Assert.Equal(59.99m, cartItem.Price);
    }

    [Fact]
    public async Task AddToCart_UsesProductDefaultsWhenDailyInventoryDoesNotExist()
    {
        var visitDate = new DateOnly(2026, 7, 4);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new Park(
                1,
                "adventurebay",
                "Adventure Bay",
                "Orlando, Florida",
                "Single-day admission, food passes, and dated add-ons.",
                new ParkCalendar(
                    [new TicketProduct("adventurebay-entry", "adventurebay", "Adventure Bay Entry", 100, 79.99m)],
                    [])));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart(
            "adventurebay",
            "adventurebay-entry",
            2,
            visitDate);

        Assert.True(result.Success);
        var cartItem = Assert.Single(shoppingService.GetCartItems(result.CartId));
        Assert.Equal("adventurebay-entry", cartItem.ProductId);
        Assert.Equal("adventurebay", cartItem.ParkUniqueName);
        Assert.Equal(visitDate, cartItem.VisitDate);
        Assert.Equal(2, cartItem.Quantity);
        Assert.Equal(79.99m, cartItem.Price);

        shoppingService.AssignTicketHolder(
            result.CartId,
            cartItem.Id,
            "Ryan",
            "Peterson",
            "ryan@example.com");

        var paymentResult = await shoppingService.MarkPaidAsync(result.CartId);

        Assert.True(paymentResult.Success);
        Assert.Equal(2, paymentResult.Tickets.Count);

        var inventory = Assert.Single(
            CreateParkService(parkRepository)
                .GetMonthlyAvailability("adventurebay", visitDate.Year, visitDate.Month),
            item => item.ProductId == "adventurebay-entry" && item.Date == visitDate);

        Assert.Equal(98, inventory.AvailableTickets);
    }

    [Fact]
    public void GetCartItems_ReturnsItemsCurrentlyInTheCart()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m),
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 2),
                10,
                0,
                89.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var firstResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 2, 2026, 7, 1);
        shoppingService.AddToCart(firstResult.CartId, "adventurebay", "adventurebay-entry", 1, 2026, 7, 2);

        var cartItems = shoppingService.GetCartItems(firstResult.CartId);

        Assert.Equal(2, cartItems.Count);

        var first = cartItems[0];
        var second = cartItems[1];

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.Equal("adventurebay-entry", first.ProductId);
        Assert.Equal("adventurebay", first.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), first.VisitDate);
        Assert.Equal(2, first.Quantity);
        Assert.Equal(79.99m, first.Price);
        Assert.Equal(159.98m, first.LineTotal);

        Assert.NotEqual(Guid.Empty, second.Id);
        Assert.Equal("adventurebay-entry", second.ProductId);
        Assert.Equal("adventurebay", second.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 2), second.VisitDate);
        Assert.Equal(1, second.Quantity);
        Assert.Equal(89.99m, second.Price);
        Assert.Equal(89.99m, second.LineTotal);
    }

    [Fact]
    public void AddToCart_AddsSameTicketAsSeparateCartItems()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var firstResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 1, 2026, 7, 1);
        shoppingService.AddToCart(firstResult.CartId, "adventurebay", "adventurebay-entry", 1, 2026, 7, 1);

        var cartItems = shoppingService.GetCartItems(firstResult.CartId);
        Assert.Equal(2, cartItems.Count);

        var first = cartItems[0];
        var second = cartItems[1];

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.Equal("adventurebay-entry", first.ProductId);
        Assert.Equal("adventurebay", first.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), first.VisitDate);
        Assert.Equal(1, first.Quantity);
        Assert.Equal(79.99m, first.Price);
        Assert.Equal(79.99m, first.LineTotal);

        Assert.NotEqual(Guid.Empty, second.Id);
        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal("adventurebay-entry", second.ProductId);
        Assert.Equal("adventurebay", second.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), second.VisitDate);
        Assert.Equal(1, second.Quantity);
        Assert.Equal(79.99m, second.Price);
        Assert.Equal(79.99m, second.LineTotal);
    }

    [Fact]
    public void AssignTicketHolder_AddsNamesAndEmailsToTwoTicketsOfTheSameType()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 1, 2026, 7, 1);
        shoppingService.AddToCart(addResult.CartId, "adventurebay", "adventurebay-entry", 1, 2026, 7, 1);
        var addedCartItems = shoppingService.GetCartItems(addResult.CartId);
        var firstCartItemId = addedCartItems[0].Id;
        var secondCartItemId = addedCartItems[1].Id;

        var firstAssignResult = shoppingService.AssignTicketHolder(
            addResult.CartId,
            firstCartItemId,
            "Ryan",
            "Peterson",
            "ryan@example.com");
        var secondAssignResult = shoppingService.AssignTicketHolder(
            addResult.CartId,
            secondCartItemId,
            "Jamie",
            "Smith",
            "jamie@example.com");

        Assert.True(firstAssignResult.Success);
        Assert.Null(firstAssignResult.FailureCode);
        Assert.Null(firstAssignResult.Message);
        Assert.True(secondAssignResult.Success);
        Assert.Null(secondAssignResult.FailureCode);
        Assert.Null(secondAssignResult.Message);

        var cartItems = shoppingService.GetCartItems(addResult.CartId);
        Assert.Equal(2, cartItems.Count);

        var first = cartItems[0];
        var second = cartItems[1];

        Assert.Equal(firstCartItemId, first.Id);
        Assert.Equal("adventurebay-entry", first.ProductId);
        Assert.Equal("Ryan", first.TicketHolderFirstName);
        Assert.Equal("Peterson", first.TicketHolderLastName);
        Assert.Equal("ryan@example.com", first.TicketHolderEmail);

        Assert.Equal(secondCartItemId, second.Id);
        Assert.Equal("adventurebay-entry", second.ProductId);
        Assert.Equal("Jamie", second.TicketHolderFirstName);
        Assert.Equal("Smith", second.TicketHolderLastName);
        Assert.Equal("jamie@example.com", second.TicketHolderEmail);
    }

    [Fact]
    public void AddToCart_ReturnsNewCartIdWhenProvidedCartDoesNotExist()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var missingCartId = Guid.NewGuid();

        var result = shoppingService.AddToCart(missingCartId, "adventurebay", "adventurebay-entry", 2, 2026, 7, 1);

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
                "adventurebay-plus",
                "adventurebay",
                "Adventure Bay Plus",
                new DateOnly(2026, 7, 1),
                5,
                4,
                99.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart("adventurebay", "adventurebay-plus", 2, 2026, 7, 1);

        Assert.False(result.Success);
        Assert.Equal(Guid.Empty, result.CartId);
        Assert.Equal("NotEnoughInventory", result.FailureCode);
        Assert.Equal("Only 1 item is available for adventurebay-plus.", result.Message);
        Assert.Empty(shoppingService.GetCartItems(result.CartId));
    }

    [Fact]
    public void AddToCart_ReturnsFailureWhenQuantityIsNotPositive()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 0, 2026, 7, 1);

        Assert.False(result.Success);
        Assert.Equal(Guid.Empty, result.CartId);
        Assert.Equal("InvalidQuantity", result.FailureCode);
        Assert.Equal("Quantity must be greater than zero.", result.Message);
    }

    [Fact]
    public void AddToCart_ReturnsFailureWhenVisitDateIsBeforeTodayInParkTimeZone()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var timeProvider = new StubTimeProvider(
            new DateTimeOffset(2026, 7, 2, 4, 0, 0, TimeSpan.Zero));
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            new StubTicketRepository(),
            new StubEmailService(),
            new TicketEmailService("tickets@parkticketsystem.test"),
            timeProvider);

        var result = shoppingService.AddToCart(
            "adventurebay",
            "adventurebay-entry",
            1,
            new DateOnly(2026, 7, 1));

        Assert.False(result.Success);
        Assert.Equal(Guid.Empty, result.CartId);
        Assert.Equal("VisitDateInPast", result.FailureCode);
        Assert.Empty(shoppingService.GetCartItems(result.CartId));
        Assert.Equal(0, cartRepository.SaveCount);
    }

    [Fact]
    public void RemoveFromCart_RemovesSelectedItemFromTheCart()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 3, 2026, 7, 1);

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
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m),
            new DailyInventory(
                "adventurebay-plus",
                "adventurebay",
                "Adventure Bay Plus",
                new DateOnly(2026, 7, 1),
                10,
                0,
                99.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 2, 2026, 7, 1);
        shoppingService.AddToCart(addResult.CartId, "adventurebay", "adventurebay-plus", 1, 2026, 7, 1);

        var removeResult = shoppingService.RemoveFromCart(addResult.CartId, "adventurebay-entry", 2026, 7, 1);

        Assert.True(removeResult.Success);
        Assert.Null(removeResult.FailureCode);
        Assert.Null(removeResult.Message);

        var cartItems = shoppingService.GetCartItems(addResult.CartId);
        var remainingItem = Assert.Single(cartItems);

        Assert.Equal("adventurebay-plus", remainingItem.ProductId);
        Assert.Equal("adventurebay", remainingItem.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), remainingItem.VisitDate);
        Assert.Equal(1, remainingItem.Quantity);
        Assert.Equal(99.99m, remainingItem.Price);
        Assert.Equal(99.99m, remainingItem.LineTotal);
    }

    [Fact]
    public async Task MarkPaid_CreatesTicketAndEmailsItToTheTicketHolder()
    {
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var ticketRepository = new StubTicketRepository();
        var emailService = new StubEmailService();
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            new TicketEmailService("tickets@parkticketsystem.test"),
            CreateTimeProvider());

        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 1, 2026, 7, 1);
        var cartItem = Assert.Single(shoppingService.GetCartItems(addResult.CartId));
        shoppingService.AssignTicketHolder(
            addResult.CartId,
            cartItem.Id,
            "Ryan",
            "Peterson",
            "ryan@example.com");

        var result = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.True(result.Success);
        Assert.Null(result.FailureCode);
        Assert.Null(result.Message);

        var ticket = Assert.Single(result.Tickets);
        Assert.NotEqual(Guid.Empty, ticket.Id);
        Assert.Equal(cartItem.Id, ticket.CartItemId);
        Assert.Equal("adventurebay-entry", ticket.ProductId);
        Assert.Equal("adventurebay", ticket.ParkUniqueName);
        Assert.Equal(new DateOnly(2026, 7, 1), ticket.VisitDate);
        Assert.Equal(79.99m, ticket.Price);
        Assert.Equal("Ryan", ticket.FirstName);
        Assert.Equal("Peterson", ticket.LastName);
        Assert.Equal("ryan@example.com", ticket.Email);

        var savedTicket = ticketRepository.GetById(ticket.Id);
        Assert.NotNull(savedTicket);
        Assert.Equal("Ryan", savedTicket.FirstName);
        Assert.Equal("Peterson", savedTicket.LastName);
        Assert.Equal("ryan@example.com", savedTicket.Email);

        var email = Assert.Single(emailService.SentEmails);
        Assert.Equal("tickets@parkticketsystem.test", email.SenderEmail);
        Assert.Equal("ryan@example.com", email.ReceiverEmail);
        Assert.Equal("Your ticket for July 1, 2026", email.Subject);
        Assert.Contains(ticket.Id.ToString(), email.PlainText);
        Assert.Contains(ticket.Id.ToString(), email.Html);
    }

    [Fact]
    public async Task MarkPaid_ReturnsFailureWithoutMarkingCartPaidWhenVisitDateIsBeforeToday()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory(
                "adventurebay-entry",
                "adventurebay",
                "Adventure Bay Entry",
                visitDate,
                10,
                0,
                79.99m));
        var ticketRepository = new StubTicketRepository();
        var emailService = new StubEmailService();
        var timeProvider = new StubTimeProvider(
            new DateTimeOffset(2026, 7, 1, 4, 0, 0, TimeSpan.Zero));
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            new TicketEmailService("tickets@parkticketsystem.test"),
            timeProvider);
        var addResult = shoppingService.AddToCart(
            "adventurebay",
            "adventurebay-entry",
            1,
            visitDate);
        var cartItem = Assert.Single(shoppingService.GetCartItems(addResult.CartId));
        shoppingService.AssignTicketHolder(
            addResult.CartId,
            cartItem.Id,
            "Ryan",
            "Peterson",
            "ryan@example.com");
        timeProvider.UtcNow = new DateTimeOffset(2026, 7, 2, 4, 0, 0, TimeSpan.Zero);

        var result = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.False(result.Success);
        Assert.Equal("VisitDateInPast", result.FailureCode);
        Assert.Empty(ticketRepository.Tickets);
        Assert.Empty(emailService.SentEmails);
        Assert.Equal(0, parkRepository.SaveCount);

        timeProvider.UtcNow = new DateTimeOffset(2026, 7, 1, 4, 0, 0, TimeSpan.Zero);
        var retryResult = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.True(retryResult.Success);
    }

    [Fact]
    public async Task MarkPaid_CreatesTicketsAndEmailsForItemsFromTwoParks()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var adventureBay = new Park(
            1,
            "adventurebay",
            "Adventure Bay",
            "Orlando, Florida",
            "Single-day admission, food passes, and dated add-ons.",
            new ParkCalendar(
            [
                new DailyInventory(
                    "adventurebay-entry",
                    "adventurebay",
                    "Adventure Bay Entry",
                    visitDate,
                    10,
                    2,
                    79.99m)
            ]));
        var splashHarbor = new Park(
            2,
            "splashharbor",
            "Splash Harbor",
            "Tampa, Florida",
            "Water park admission, food passes, and dated add-ons.",
            new ParkCalendar(
            [
                new DailyInventory(
                    "splashharbor-entry",
                    "splashharbor",
                    "Splash Harbor Entry",
                    visitDate,
                    20,
                    5,
                    59.99m)
            ]));
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(adventureBay, splashHarbor);
        var ticketRepository = new StubTicketRepository();
        var emailService = new StubEmailService();
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            new TicketEmailService("tickets@parkticketsystem.test"),
            CreateTimeProvider());
        var addResult = shoppingService.AddToCart(
            "adventurebay",
            "adventurebay-entry",
            1,
            visitDate);
        shoppingService.AddToCart(
            addResult.CartId,
            "splashharbor",
            "splashharbor-entry",
            1,
            visitDate);

        foreach (var cartItem in shoppingService.GetCartItems(addResult.CartId))
        {
            shoppingService.AssignTicketHolder(
                addResult.CartId,
                cartItem.Id,
                "Ryan",
                "Peterson",
                "ryan@example.com");
        }

        var result = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Tickets.Count);
        Assert.Equal("adventurebay", result.Tickets[0].ParkUniqueName);
        Assert.Equal("splashharbor", result.Tickets[1].ParkUniqueName);
        Assert.Equal(2, ticketRepository.Tickets.Count);
        Assert.Equal(2, emailService.SentEmails.Count);

        var parkService = CreateParkService(parkRepository);
        var adventureBayAvailability = Assert.Single(
            parkService.GetMonthlyAvailability("adventurebay", 2026, 7));
        var splashHarborAvailability = Assert.Single(
            parkService.GetMonthlyAvailability("splashharbor", 2026, 7));

        Assert.Equal(7, adventureBayAvailability.AvailableTickets);
        Assert.Equal(14, splashHarborAvailability.AvailableTickets);
    }

    [Fact]
    public async Task MarkPaid_UpdatesParkInventoryForPurchasedTicketTypes()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var otherDate = new DateOnly(2026, 7, 2);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", visitDate, 10, 2, 79.99m),
            new DailyInventory("adventurebay-plus", "adventurebay", "Adventure Bay Plus", visitDate, 10, 1, 99.99m),
            new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", otherDate, 10, 3, 89.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 2, visitDate);
        shoppingService.AddToCart(addResult.CartId, "adventurebay", "adventurebay-plus", 1, visitDate);

        foreach (var cartItem in shoppingService.GetCartItems(addResult.CartId))
        {
            shoppingService.AssignTicketHolder(
                addResult.CartId,
                cartItem.Id,
                "Ryan",
                "Peterson",
                "ryan@example.com");
        }

        var result = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.True(result.Success);

        var availability = CreateParkService(parkRepository)
            .GetMonthlyAvailability("adventurebay", 2026, 7);

        Assert.Equal(6, availability[0].AvailableTickets);
        Assert.Equal(8, availability[1].AvailableTickets);
        Assert.Equal(7, availability[2].AvailableTickets);
    }

    [Fact]
    public async Task MarkPaid_ReturnsFailureWithoutCreatingTicketsOrUpdatingInventoryWhenInventoryWasSoldByAnotherCart()
    {
        var soldOutDate = new DateOnly(2026, 7, 1);
        var availableDate = new DateOnly(2026, 7, 2);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", soldOutDate, 1, 0, 79.99m),
            new DailyInventory("adventurebay-entry", "adventurebay", "Adventure Bay Entry", availableDate, 10, 0, 89.99m));
        var ticketRepository = new StubTicketRepository();
        var emailService = new StubEmailService();
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            new TicketEmailService("tickets@parkticketsystem.test"),
            CreateTimeProvider());

        var firstCart = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 1, soldOutDate);
        var secondCart = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 1, soldOutDate);
        shoppingService.AddToCart(secondCart.CartId, "adventurebay", "adventurebay-entry", 1, availableDate);

        foreach (var cartItem in shoppingService.GetCartItems(firstCart.CartId))
        {
            shoppingService.AssignTicketHolder(
                firstCart.CartId,
                cartItem.Id,
                "Ryan",
                "Peterson",
                "ryan@example.com");
        }

        foreach (var cartItem in shoppingService.GetCartItems(secondCart.CartId))
        {
            shoppingService.AssignTicketHolder(
                secondCart.CartId,
                cartItem.Id,
                "Jamie",
                "Smith",
                "jamie@example.com");
        }

        var firstPayment = await shoppingService.MarkPaidAsync(firstCart.CartId);
        var cartSaveCount = cartRepository.SaveCount;
        var parkSaveCount = parkRepository.SaveCount;
        var ticketSaveCount = ticketRepository.SaveCount;
        var secondPayment = await shoppingService.MarkPaidAsync(secondCart.CartId);

        Assert.True(firstPayment.Success);
        Assert.False(secondPayment.Success);
        Assert.Equal("NotEnoughInventory", secondPayment.FailureCode);
        Assert.Empty(secondPayment.Tickets);
        Assert.Single(ticketRepository.Tickets);
        Assert.Single(emailService.SentEmails);
        Assert.Equal(cartSaveCount, cartRepository.SaveCount);
        Assert.Equal(parkSaveCount, parkRepository.SaveCount);
        Assert.Equal(ticketSaveCount, ticketRepository.SaveCount);

        var availability = CreateParkService(parkRepository)
            .GetMonthlyAvailability("adventurebay", 2026, 7);
        var soldOutInventory = Assert.Single(
            availability,
            inventory => inventory.ProductId == "adventurebay-entry" && inventory.Date == soldOutDate);
        var availableInventory = Assert.Single(
            availability,
            inventory => inventory.ProductId == "adventurebay-entry" && inventory.Date == availableDate);

        Assert.Equal(0, soldOutInventory.AvailableTickets);
        Assert.Equal(10, availableInventory.AvailableTickets);
    }

    [Fact]
    public async Task MarkPaid_CreatesDailyInventoryFromProductDefaults()
    {
        var visitDate = new DateOnly(2026, 7, 4);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new Park(
                1,
                "adventurebay",
                "Adventure Bay",
                "Orlando, Florida",
                "Single-day admission, food passes, and dated add-ons.",
                new ParkCalendar(
                    [new TicketProduct("adventurebay-entry", "adventurebay", "Adventure Bay Entry", 100, 79.99m)],
                    [])));
        var ticketRepository = new StubTicketRepository();
        var emailService = new StubEmailService();
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            new TicketEmailService("tickets@parkticketsystem.test"),
            CreateTimeProvider());
        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 2, visitDate);
        var cartItem = Assert.Single(shoppingService.GetCartItems(addResult.CartId));

        Assert.Equal(79.99m, cartItem.Price);

        shoppingService.AssignTicketHolder(
            addResult.CartId,
            cartItem.Id,
            "Ryan",
            "Peterson",
            "ryan@example.com");

        var result = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.True(result.Success);
        Assert.Equal(2, result.Tickets.Count);
        Assert.Equal(2, ticketRepository.Tickets.Count);
        Assert.Equal(2, emailService.SentEmails.Count);
        Assert.Equal(1, parkRepository.SaveCount);
    }

    [Fact]
    public async Task MarkPaid_UpdatesExistingAndDefaultProductInventory()
    {
        var visitDate = new DateOnly(2026, 7, 4);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new Park(
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
                    ])));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay", "adventurebay-entry", 1, visitDate);
        shoppingService.AddToCart(addResult.CartId, "adventurebay", "adventurebay-plus", 2, visitDate);
        var cartItems = shoppingService.GetCartItems(addResult.CartId);

        Assert.Equal(89.99m, cartItems[0].Price);
        Assert.Equal(99.99m, cartItems[1].Price);

        foreach (var cartItem in cartItems)
        {
            shoppingService.AssignTicketHolder(
                addResult.CartId,
                cartItem.Id,
                "Ryan",
                "Peterson",
                "ryan@example.com");
        }

        var result = await shoppingService.MarkPaidAsync(addResult.CartId);

        Assert.True(result.Success);
        Assert.Equal(3, result.Tickets.Count);

        var availability = CreateParkService(parkRepository)
            .GetMonthlyAvailability("adventurebay", visitDate.Year, visitDate.Month);
        var entryInventory = Assert.Single(
            availability,
            inventory => inventory.ProductId == "adventurebay-entry" && inventory.Date == visitDate);
        var plusInventory = Assert.Single(
            availability,
            inventory => inventory.ProductId == "adventurebay-plus" && inventory.Date == visitDate);

        Assert.Equal("Adventure Bay Entry", entryInventory.ProductName);
        Assert.Equal(visitDate, entryInventory.Date);
        Assert.Equal(7, entryInventory.AvailableTickets);
        Assert.Equal(89.99m, entryInventory.Price);

        Assert.Equal("Adventure Bay Plus", plusInventory.ProductName);
        Assert.Equal(visitDate, plusInventory.Date);
        Assert.Equal(48, plusInventory.AvailableTickets);
        Assert.Equal(99.99m, plusInventory.Price);
    }

    private static ShoppingService CreateShoppingService(
        ICartRepository cartRepository,
        IParkRepository parkRepository)
    {
        return new ShoppingService(
            cartRepository,
            parkRepository,
            new StubTicketRepository(),
            new StubEmailService(),
            new TicketEmailService("tickets@parkticketsystem.test"),
            CreateTimeProvider());
    }

    private static StubTimeProvider CreateTimeProvider()
    {
        return new StubTimeProvider(
            new DateTimeOffset(2026, 7, 1, 4, 0, 0, TimeSpan.Zero));
    }

    private sealed class StubCartRepository : ICartRepository
    {
        private readonly Dictionary<Guid, Cart> _carts = [];
        private readonly Dictionary<Guid, Cart> _stagedCarts = [];

        public int SaveCount { get; private set; }

        public Cart? GetById(Guid cartId)
        {
            return _carts.GetValueOrDefault(cartId);
        }

        public void Save(Cart cart)
        {
            SaveCount++;
            _stagedCarts[cart.Id] = cart;
        }

        public void Commit()
        {
            foreach (var cart in _stagedCarts)
            {
                _carts[cart.Key] = cart.Value;
            }

            _stagedCarts.Clear();
        }
    }

    private static ParkService CreateParkService(IParkRepository parkRepository)
    {
        return new ParkService(
            parkRepository,
            new StubTimeProvider(new DateTimeOffset(2026, 7, 1, 4, 0, 0, TimeSpan.Zero)));
    }

    private sealed class StubParkRepository : IParkRepository
    {
        private readonly List<Park> _parks;
        private readonly Dictionary<string, Park> _stagedParks = [];

        public int SaveCount { get; private set; }

        public StubParkRepository(params DailyInventory[] products)
        {
            _parks =
            [
                new Park(
                    1,
                    "adventurebay",
                    "Adventure Bay",
                    "Orlando, Florida",
                    "Single-day admission, food passes, and dated add-ons.",
                    new ParkCalendar(products))
            ];
        }

        public StubParkRepository(params Park[] parks)
        {
            _parks = parks.Select(Clone).ToList();
        }

        public IReadOnlyList<Park> GetAll()
        {
            return _parks.Select(Clone).ToArray();
        }

        public Park? GetByUniqueName(string uniqueName)
        {
            var park = _parks.SingleOrDefault(park => park.UniqueName == uniqueName);
            return park is null ? null : Clone(park);
        }

        public void Save(Park park)
        {
            SaveCount++;
            _stagedParks[park.UniqueName] = Clone(park);
        }

        public void Commit()
        {
            foreach (var stagedPark in _stagedParks.Values)
            {
                var index = _parks.FindIndex(park => park.UniqueName == stagedPark.UniqueName);

                if (index >= 0)
                {
                    _parks[index] = stagedPark;
                }
                else
                {
                    _parks.Add(stagedPark);
                }
            }

            _stagedParks.Clear();
        }

        private static Park Clone(Park park)
        {
            return new Park(
                park.Id,
                park.UniqueName,
                park.Name,
                park.Location,
                park.TagText,
                park.TimeZoneId,
                new ParkCalendar(
                    park.Calendar.TicketProducts.Select(product => new TicketProduct(
                        product.Id,
                        product.ParkUniqueName,
                        product.Name,
                        product.DefaultAvailableTicketCount,
                        product.Price)),
                    park.Calendar.DailyInventories.Select(inventory => new DailyInventory(
                        inventory.ProductId,
                        inventory.ParkUniqueName,
                        inventory.Name,
                        inventory.Date,
                        inventory.TotalItemCount,
                        inventory.SoldItemCount,
                        inventory.Price))));
        }
    }

    private sealed class StubTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = utcNow;

        public override DateTimeOffset GetUtcNow()
        {
            return UtcNow;
        }
    }

    private sealed class StubTicketRepository : ITicketRepository
    {
        private readonly Dictionary<Guid, Ticket> _tickets = [];
        private readonly Dictionary<Guid, Ticket> _stagedTickets = [];

        public IReadOnlyCollection<Ticket> Tickets => _tickets.Values;

        public int SaveCount { get; private set; }

        public Ticket? GetById(Guid ticketId)
        {
            return _tickets.GetValueOrDefault(ticketId);
        }

        public void Save(Ticket ticket)
        {
            SaveCount++;
            _stagedTickets[ticket.Id] = ticket;
        }

        public void Commit()
        {
            foreach (var ticket in _stagedTickets)
            {
                _tickets[ticket.Key] = ticket.Value;
            }

            _stagedTickets.Clear();
        }
    }

    private sealed class StubEmailService : IEmailService
    {
        private readonly List<TicketEmail> _sentEmails = [];

        public IReadOnlyList<TicketEmail> SentEmails => _sentEmails;

        public Task SendAsync(
            string senderEmail,
            string receiverEmail,
            string subject,
            string plainText,
            string html,
            CancellationToken cancellationToken = default)
        {
            _sentEmails.Add(new TicketEmail(senderEmail, receiverEmail, subject, plainText, html));
            return Task.CompletedTask;
        }
    }

}
