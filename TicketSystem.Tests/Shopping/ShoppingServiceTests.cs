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
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var result = shoppingService.AddToCart("adventurebay-entry", 3, 2026, 7, 1);

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
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var firstResult = shoppingService.AddToCart("adventurebay-entry", 2, 2026, 7, 1);
        shoppingService.AddToCart(firstResult.CartId, "adventurebay-entry", 1, 2026, 7, 2);

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
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

        var firstResult = shoppingService.AddToCart("adventurebay-entry", 1, 2026, 7, 1);
        shoppingService.AddToCart(firstResult.CartId, "adventurebay-entry", 1, 2026, 7, 1);

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
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay-entry", 1, 2026, 7, 1);
        shoppingService.AddToCart(addResult.CartId, "adventurebay-entry", 1, 2026, 7, 1);
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
                "Park Ticket",
                new DateOnly(2026, 7, 1),
                10,
                0,
                79.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
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
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);

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
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
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
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
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

    [Fact]
    public async Task MarkPaid_CreatesTicketAndEmailsItToTheTicketHolder()
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
        var ticketRepository = new StubTicketRepository();
        var emailService = new StubEmailService();
        var shoppingService = new ShoppingService(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            new TicketEmailService("tickets@parkticketsystem.test"));

        var addResult = shoppingService.AddToCart("adventurebay-entry", 1, 2026, 7, 1);
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
    public async Task MarkPaid_UpdatesParkInventoryForPurchasedTicketTypes()
    {
        var visitDate = new DateOnly(2026, 7, 1);
        var otherDate = new DateOnly(2026, 7, 2);
        var cartRepository = new StubCartRepository();
        var parkRepository = new StubParkRepository(
            new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", visitDate, 10, 2, 79.99m),
            new DailyInventory("adventurebay-food", "adventurebay", "Food Pass", visitDate, 10, 1, 29.99m),
            new DailyInventory("adventurebay-entry", "adventurebay", "Park Ticket", otherDate, 10, 3, 89.99m));
        var shoppingService = CreateShoppingService(cartRepository, parkRepository);
        var addResult = shoppingService.AddToCart("adventurebay-entry", 2, visitDate);
        shoppingService.AddToCart(addResult.CartId, "adventurebay-food", 1, visitDate);

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

        var availability = new ParkService(parkRepository)
            .GetMonthlyAvailability("adventurebay", 2026, 7);

        Assert.Equal(6, availability[0].AvailableTickets);
        Assert.Equal(8, availability[1].AvailableTickets);
        Assert.Equal(7, availability[2].AvailableTickets);
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
            new TicketEmailService("tickets@parkticketsystem.test"));
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
        private Park _park = new(
            1,
            "adventurebay",
            "Adventure Bay",
            new ParkCalendar(products));

        public Park? GetByUniqueName(string uniqueName)
        {
            return _park.UniqueName == uniqueName ? Clone(_park) : null;
        }

        public void Save(Park park)
        {
            _park = Clone(park);
        }

        private static Park Clone(Park park)
        {
            return new Park(
                park.Id,
                park.UniqueName,
                park.Name,
                new ParkCalendar(park.Calendar.DailyInventories.Select(inventory => new DailyInventory(
                    inventory.ProductId,
                    inventory.ParkUniqueName,
                    inventory.Name,
                    inventory.Date,
                    inventory.TotalItemCount,
                    inventory.SoldItemCount,
                    inventory.Price))));
        }
    }

    private sealed class StubTicketRepository : ITicketRepository
    {
        private readonly Dictionary<Guid, Ticket> _tickets = [];

        public Ticket? GetById(Guid ticketId)
        {
            return _tickets.GetValueOrDefault(ticketId);
        }

        public void Save(Ticket ticket)
        {
            _tickets[ticket.Id] = ticket;
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
