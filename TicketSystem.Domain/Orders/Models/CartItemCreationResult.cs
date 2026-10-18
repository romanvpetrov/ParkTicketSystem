namespace TicketSystem.Domain.Orders;

public enum CartItemCreationFailure
{
    None,
    ProductNotFound,
    InvalidQuantity,
    NotEnoughInventory
}

public sealed record CartItemCreationResult(
    bool Success,
    CartItem? CartItem,
    CartItemCreationFailure Failure,
    int AvailableItemCount)
{
    public static CartItemCreationResult Succeeded(CartItem cartItem)
    {
        return new CartItemCreationResult(true, cartItem, CartItemCreationFailure.None, 0);
    }

    public static CartItemCreationResult Failed(
        CartItemCreationFailure failure,
        int availableItemCount = 0)
    {
        return new CartItemCreationResult(false, null, failure, availableItemCount);
    }
}
