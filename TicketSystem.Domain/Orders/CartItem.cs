namespace TicketSystem.Domain.Orders;

public sealed class CartItem
{
    public CartItem(string productId, string parkUniqueName, DateOnly visitDate, int quantity, decimal price)
    {
        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(parkUniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(parkUniqueName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        ProductId = productId;
        ParkUniqueName = parkUniqueName;
        VisitDate = visitDate;
        Quantity = quantity;
        Price = price;
    }

    public string ProductId { get; }

    public string ParkUniqueName { get; }

    public DateOnly VisitDate { get; }

    public int Quantity { get; }

    public decimal Price { get; }

    public decimal LineTotal => Quantity * Price;

    public CartItem IncreaseQuantityBy(int quantity)
    {
        return new CartItem(ProductId, ParkUniqueName, VisitDate, Quantity + quantity, Price);
    }
}
