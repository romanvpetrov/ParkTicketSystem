using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Parks;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Services.Shopping;

public sealed class ShoppingService
{
    private readonly ICartRepository _cartRepository;
    private readonly IParkRepository _parkRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IEmailService _emailService;
    private readonly TicketEmailService _ticketEmailService;
    private readonly TimeProvider _timeProvider;

    public ShoppingService(
        ICartRepository cartRepository,
        IParkRepository parkRepository,
        ITicketRepository ticketRepository,
        IEmailService emailService,
        TicketEmailService ticketEmailService)
        : this(
            cartRepository,
            parkRepository,
            ticketRepository,
            emailService,
            ticketEmailService,
            TimeProvider.System)
    {
    }

    public ShoppingService(
        ICartRepository cartRepository,
        IParkRepository parkRepository,
        ITicketRepository ticketRepository,
        IEmailService emailService,
        TicketEmailService ticketEmailService,
        TimeProvider timeProvider)
    {
        _cartRepository = cartRepository;
        _parkRepository = parkRepository;
        _ticketRepository = ticketRepository;
        _emailService = emailService;
        _ticketEmailService = ticketEmailService;
        _timeProvider = timeProvider;
    }

    public AddToCartResult AddToCart(string parkUniqueName, string productId, int quantity)
    {
        var park = _parkRepository.GetByUniqueName(parkUniqueName);

        if (park is null)
        {
            return AddToCartResult.Failed("ParkNotFound", $"Park '{parkUniqueName}' was not found.");
        }

        return AddToCart(
            parkUniqueName,
            productId,
            quantity,
            park.GetCurrentDate(_timeProvider.GetUtcNow()));
    }

    public AddToCartResult AddToCart(
        string parkUniqueName,
        string productId,
        int quantity,
        int year,
        int month,
        int day)
    {
        return AddToCart(parkUniqueName, productId, quantity, new DateOnly(year, month, day));
    }

    public AddToCartResult AddToCart(
        Guid cartId,
        string parkUniqueName,
        string productId,
        int quantity,
        int year,
        int month,
        int day)
    {
        return AddToCart(cartId, parkUniqueName, productId, quantity, new DateOnly(year, month, day));
    }

    public AddToCartResult AddToCart(
        string parkUniqueName,
        string productId,
        int quantity,
        DateOnly visitDate)
    {
        var result = CreateCartItem(parkUniqueName, productId, quantity, visitDate);

        if (!result.Success)
        {
            return AddToCartResult.Failed(result.FailureCode!, result.Message!);
        }

        var cart = new Cart(Guid.NewGuid());
        cart.AddItem(result.CartItem!);
        _cartRepository.Save(cart);
        _cartRepository.Commit();

        return AddToCartResult.Succeeded(cart.Id);
    }

    public AddToCartResult AddToCart(
        Guid cartId,
        string parkUniqueName,
        string productId,
        int quantity,
        DateOnly visitDate)
    {
        var result = CreateCartItem(parkUniqueName, productId, quantity, visitDate);

        if (!result.Success)
        {
            return AddToCartResult.Failed(result.FailureCode!, result.Message!);
        }

        var cart = _cartRepository.GetById(cartId);

        if (cart is null)
        {
            return AddToCart(parkUniqueName, productId, quantity, visitDate);
        }

        cart.AddItem(result.CartItem!);
        _cartRepository.Save(cart);
        _cartRepository.Commit();

        return AddToCartResult.Succeeded(cartId);
    }

    public IReadOnlyList<CartItemDto> GetCartItems(Guid cartId)
    {
        var cart = _cartRepository.GetById(cartId);

        if (cart is null)
        {
            return [];
        }

        return cart.Items
            .Select(cartItem => new CartItemDto(
                cartItem.Id,
                cartItem.ProductId,
                cartItem.ParkUniqueName,
                cartItem.VisitDate,
                cartItem.Quantity,
                cartItem.Price,
                cartItem.LineTotal,
                cartItem.TicketHolder?.FirstName,
                cartItem.TicketHolder?.LastName,
                cartItem.TicketHolder?.Email))
            .ToArray();
    }

    public AssignTicketHolderResult AssignTicketHolder(
        Guid cartId,
        Guid cartItemId,
        string firstName,
        string lastName,
        string email)
    {
        var cart = _cartRepository.GetById(cartId);

        if (cart is null)
        {
            return AssignTicketHolderResult.Failed("CartNotFound", $"Cart '{cartId}' was not found.");
        }

        var ticketHolder = new TicketHolder(firstName, lastName, email);

        if (!cart.AssignTicketHolder(cartItemId, ticketHolder))
        {
            return AssignTicketHolderResult.Failed("ItemNotFound", $"Cart item '{cartItemId}' was not found in cart.");
        }

        _cartRepository.Save(cart);
        _cartRepository.Commit();

        return AssignTicketHolderResult.Succeeded();
    }

    public async Task<MarkPaidResult> MarkPaidAsync(
        Guid cartId,
        CancellationToken cancellationToken = default)
    {
        var cart = _cartRepository.GetById(cartId);

        if (cart is null)
        {
            return MarkPaidResult.Failed("CartNotFound", $"Cart '{cartId}' was not found.");
        }

        var parks = new List<Park>();
        var utcNow = _timeProvider.GetUtcNow();

        foreach (var cartItemsByPark in cart.Items.GroupBy(item => item.ParkUniqueName))
        {
            var park = _parkRepository.GetByUniqueName(cartItemsByPark.Key)
                ?? throw new InvalidOperationException($"Park '{cartItemsByPark.Key}' was not found.");

            if (cartItemsByPark.Any(item => park.IsVisitDateInPast(item, utcNow)))
            {
                return MarkPaidResult.Failed(
                    "VisitDateInPast",
                    "The cart contains a visit date that is earlier than today.");
            }

            parks.Add(park);
        }

        var paymentResult = cart.MarkPaid();

        if (!paymentResult.Success)
        {
            return paymentResult.FailureCode switch
            {
                "CartAlreadyPaid" => MarkPaidResult.Failed("CartAlreadyPaid", "The cart has already been paid."),
                "CartIsEmpty" => MarkPaidResult.Failed("CartIsEmpty", "The cart does not contain any tickets."),
                "TicketHolderRequired" => MarkPaidResult.Failed(
                    "TicketHolderRequired",
                    "Every ticket must have a ticket holder before payment can be completed."),
                _ => MarkPaidResult.Failed("PaymentFailed", "The cart could not be marked as paid.")
            };
        }

        foreach (var park in parks)
        {
            var inventoryResult = park.RecordTicketsSold(paymentResult.Tickets);

            if (!inventoryResult.Success)
            {
                return inventoryResult.Failure switch
                {
                    RecordTicketsSoldFailure.ProductNotFound => MarkPaidResult.Failed(
                        "ProductNotFound",
                        $"Product '{inventoryResult.ProductId}' was not found for {inventoryResult.VisitDate}."),
                    RecordTicketsSoldFailure.NotEnoughInventory => MarkPaidResult.Failed(
                        "NotEnoughInventory",
                        $"Only {inventoryResult.AvailableItemCount} item is available for {inventoryResult.ProductId}."),
                    _ => MarkPaidResult.Failed(
                        "InventoryUpdateFailed",
                        "Inventory could not be updated for the cart.")
                };
            }
        }

        _cartRepository.Save(cart);

        foreach (var park in parks)
        {
            _parkRepository.Save(park);
        }

        foreach (var ticket in paymentResult.Tickets)
        {
            _ticketRepository.Save(ticket);
        }

        _cartRepository.Commit();
        _parkRepository.Commit();
        _ticketRepository.Commit();

        foreach (var ticket in paymentResult.Tickets)
        {
            var email = _ticketEmailService.Create(ticket);
            await _emailService.SendAsync(
                email.SenderEmail,
                email.ReceiverEmail,
                email.Subject,
                email.PlainText,
                email.Html,
                cancellationToken);
        }

        var tickets = paymentResult.Tickets
            .Select(ticket => new TicketDto(
                ticket.Id,
                ticket.CartItemId,
                ticket.ProductId,
                ticket.ParkUniqueName,
                ticket.VisitDate,
                ticket.Price,
                ticket.FirstName,
                ticket.LastName,
                ticket.Email))
            .ToArray();

        return MarkPaidResult.Succeeded(tickets);
    }

    public RemoveFromCartResult RemoveFromCart(Guid cartId, string productId)
    {
        return RemoveFromCart(cartId, productId, DateOnly.FromDateTime(DateTime.Today));
    }

    public RemoveFromCartResult RemoveFromCart(Guid cartId, string productId, int year, int month, int day)
    {
        return RemoveFromCart(cartId, productId, new DateOnly(year, month, day));
    }

    public RemoveFromCartResult RemoveFromCart(Guid cartId, string productId, DateOnly visitDate)
    {
        var cart = _cartRepository.GetById(cartId);

        if (cart is null)
        {
            return RemoveFromCartResult.Failed("CartNotFound", $"Cart '{cartId}' was not found.");
        }

        if (!cart.RemoveItem(productId, visitDate))
        {
            return RemoveFromCartResult.Failed("ItemNotFound", $"Product '{productId}' was not found in cart.");
        }

        _cartRepository.Save(cart);
        _cartRepository.Commit();

        return RemoveFromCartResult.Succeeded();
    }

    private CreateCartItemResult CreateCartItem(
        string parkUniqueName,
        string productId,
        int quantity,
        DateOnly visitDate)
    {
        var park = _parkRepository.GetByUniqueName(parkUniqueName);

        if (park is null)
        {
            return CreateCartItemResult.Failed("ParkNotFound", $"Park '{parkUniqueName}' was not found.");
        }

        var creationResult = park.TryCreateCartItem(productId, visitDate, quantity);

        if (creationResult.Success)
        {
            if (park.IsVisitDateInPast(creationResult.CartItem!, _timeProvider.GetUtcNow()))
            {
                return CreateCartItemResult.Failed(
                    "VisitDateInPast",
                    $"Visit date '{visitDate}' is earlier than today.");
            }

            return CreateCartItemResult.Succeeded(creationResult.CartItem!);
        }

        return creationResult.Failure switch
        {
            CartItemCreationFailure.ProductNotFound => CreateCartItemResult.Failed(
                "ProductNotFound",
                $"Product '{productId}' was not found for {visitDate}."),
            CartItemCreationFailure.InvalidQuantity => CreateCartItemResult.Failed(
                "InvalidQuantity",
                "Quantity must be greater than zero."),
            CartItemCreationFailure.NotEnoughInventory => CreateCartItemResult.Failed(
                "NotEnoughInventory",
                $"Only {creationResult.AvailableItemCount} item is available for {productId}."),
            _ => CreateCartItemResult.Failed(
                "CartItemCreationFailed",
                "The cart item could not be created.")
        };
    }

    private sealed record CreateCartItemResult(
        bool Success,
        CartItem? CartItem,
        string? FailureCode,
        string? Message)
    {
        public static CreateCartItemResult Succeeded(CartItem cartItem)
        {
            return new CreateCartItemResult(true, cartItem, null, null);
        }

        public static CreateCartItemResult Failed(string failureCode, string message)
        {
            return new CreateCartItemResult(false, null, failureCode, message);
        }
    }
}
