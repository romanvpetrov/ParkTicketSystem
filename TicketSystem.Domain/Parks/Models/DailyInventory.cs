using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Parks;

public sealed class DailyInventory
{
    public DailyInventory(
        string productId,
        string parkUniqueName,
        string name,
        DateOnly date,
        int totalItemCount,
        int soldItemCount,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(parkUniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(parkUniqueName));
        }

        if (totalItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalItemCount), "Item count cannot be negative.");
        }

        if (soldItemCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(soldItemCount), "Sold item count cannot be negative.");
        }

        if (soldItemCount > totalItemCount)
        {
            throw new ArgumentException("Sold item count cannot exceed item count.", nameof(soldItemCount));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        ProductId = productId;
        ParkUniqueName = parkUniqueName;
        Name = name;
        Date = date;
        TotalItemCount = totalItemCount;
        SoldItemCount = soldItemCount;
        Price = price;
    }

    public string ProductId { get; }

    public string ParkUniqueName { get; }

    public string Name { get; }

    public DateOnly Date { get; }

    public int TotalItemCount { get; }

    public int SoldItemCount { get; private set; }

    public decimal Price { get; }

    public int AvailableItemCount => TotalItemCount - SoldItemCount;

    public CartItemCreationResult TryCreateCartItem(int quantity)
    {
        if (quantity <= 0)
        {
            return CartItemCreationResult.Failed(CartItemCreationFailure.InvalidQuantity);
        }

        if (quantity > AvailableItemCount)
        {
            return CartItemCreationResult.Failed(
                CartItemCreationFailure.NotEnoughInventory,
                AvailableItemCount);
        }

        return CartItemCreationResult.Succeeded(new CartItem(
            ProductId,
            ParkUniqueName,
            Date,
            quantity,
            Price));
    }

    public RecordItemsSoldResult TryRecordItemsSold(int quantity)
    {
        if (quantity <= 0)
        {
            return RecordItemsSoldResult.Failed(
                RecordItemsSoldFailure.InvalidQuantity,
                AvailableItemCount);
        }

        if (quantity > AvailableItemCount)
        {
            return RecordItemsSoldResult.Failed(
                RecordItemsSoldFailure.NotEnoughInventory,
                AvailableItemCount);
        }

        SoldItemCount += quantity;

        return RecordItemsSoldResult.Succeeded(AvailableItemCount);
    }
}
