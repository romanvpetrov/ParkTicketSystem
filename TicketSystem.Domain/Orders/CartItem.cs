namespace TicketSystem.Domain.Orders;

public sealed class CartItem
{
    public CartItem(string productId, string parkUniqueName, DateOnly visitDate, int quantity, decimal price)
        : this(Guid.NewGuid(), productId, parkUniqueName, visitDate, quantity, price)
    {
    }

    public CartItem(Guid id, string productId, string parkUniqueName, DateOnly visitDate, int quantity, decimal price)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Cart item id is required.", nameof(id));
        }

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

        Id = id;
        ProductId = productId;
        ParkUniqueName = parkUniqueName;
        VisitDate = visitDate;
        Quantity = quantity;
        Price = price;
    }

    public Guid Id { get; }

    public string ProductId { get; }

    public string ParkUniqueName { get; }

    public DateOnly VisitDate { get; }

    public int Quantity { get; }

    public decimal Price { get; }

    public decimal LineTotal => Quantity * Price;

    public TicketHolder? TicketHolder { get; private set; }

    public void AssignTicketHolder(TicketHolder ticketHolder)
    {
        TicketHolder = ticketHolder;
    }

    public CartItem IncreaseQuantityBy(int quantity)
    {
        return new CartItem(Id, ProductId, ParkUniqueName, VisitDate, Quantity + quantity, Price);
    }
}
