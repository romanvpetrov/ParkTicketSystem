using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;

namespace TicketSystem.Services.Shopping;

public sealed class ShoppingService
{
    private readonly ICartRepository _cartRepository;
    private readonly IParkRepository _parkRepository;

    public ShoppingService(ICartRepository cartRepository, IParkRepository parkRepository)
    {
        _cartRepository = cartRepository;
        _parkRepository = parkRepository;
    }

    public AddToCartResult AddToCart(string productId, int quantity)
    {
        return AddToCart(productId, quantity, DateOnly.FromDateTime(DateTime.Today));
    }

    public AddToCartResult AddToCart(string productId, int quantity, int year, int month, int day)
    {
        return AddToCart(productId, quantity, new DateOnly(year, month, day));
    }

    public AddToCartResult AddToCart(Guid cartId, string productId, int quantity, int year, int month, int day)
    {
        return AddToCart(cartId, productId, quantity, new DateOnly(year, month, day));
    }

    public AddToCartResult AddToCart(string productId, int quantity, DateOnly visitDate)
    {
        var result = CreateCartItem(productId, quantity, visitDate);

        if (!result.Success)
        {
            return AddToCartResult.Failed(result.FailureCode!, result.Message!);
        }

        var cart = new Cart(Guid.NewGuid());
        cart.AddItem(result.CartItem!);
        _cartRepository.Save(cart);

        return AddToCartResult.Succeeded(cart.Id);
    }

    public AddToCartResult AddToCart(Guid cartId, string productId, int quantity, DateOnly visitDate)
    {
        var result = CreateCartItem(productId, quantity, visitDate);

        if (!result.Success)
        {
            return AddToCartResult.Failed(result.FailureCode!, result.Message!);
        }

        var cart = _cartRepository.GetById(cartId);

        if (cart is null)
        {
            return AddToCart(productId, quantity, visitDate);
        }

        cart.AddItem(result.CartItem!);
        _cartRepository.Save(cart);

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
                cartItem.ProductId,
                cartItem.ParkUniqueName,
                cartItem.VisitDate,
                cartItem.Quantity,
                cartItem.Price,
                cartItem.LineTotal))
            .ToArray();
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

        return RemoveFromCartResult.Succeeded();
    }

    private CreateCartItemResult CreateCartItem(string productId, int quantity, DateOnly visitDate)
    {
        var park = _parkRepository.GetByUniqueName("adventurebay");

        if (park is null)
        {
            return CreateCartItemResult.Failed("ParkNotFound", "Park 'adventurebay' was not found.");
        }

        var product = park.GetDailyInventory(productId, visitDate);

        if (product is null)
        {
            return CreateCartItemResult.Failed("ProductNotFound", $"Product '{productId}' was not found for {visitDate}.");
        }

        if (quantity > product.AvailableItemCount)
        {
            return CreateCartItemResult.Failed(
                "NotEnoughInventory",
                $"Only {product.AvailableItemCount} item is available for {product.ProductId}.");
        }

        var cartItem = new CartItem(
            product.ProductId,
            product.ParkUniqueName,
            product.Date,
            quantity,
            product.Price);

        return CreateCartItemResult.Succeeded(cartItem);
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
