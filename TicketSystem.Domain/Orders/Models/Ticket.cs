namespace TicketSystem.Domain.Orders;

public sealed class Ticket
{
    public Ticket(
        Guid id,
        Guid cartId,
        Guid cartItemId,
        string productId,
        string parkUniqueName,
        DateOnly visitDate,
        decimal price,
        string firstName,
        string lastName,
        string email)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Ticket id is required.", nameof(id));
        }

        if (cartId == Guid.Empty)
        {
            throw new ArgumentException("Cart id is required.", nameof(cartId));
        }

        if (cartItemId == Guid.Empty)
        {
            throw new ArgumentException("Cart item id is required.", nameof(cartItemId));
        }

        if (string.IsNullOrWhiteSpace(productId))
        {
            throw new ArgumentException("Product id is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(parkUniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(parkUniqueName));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name is required.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("Last name is required.", nameof(lastName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        Id = id;
        CartId = cartId;
        CartItemId = cartItemId;
        ProductId = productId;
        ParkUniqueName = parkUniqueName;
        VisitDate = visitDate;
        Price = price;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public Guid Id { get; }
    public Guid CartId { get; }
    public Guid CartItemId { get; }
    public string ProductId { get; }
    public string ParkUniqueName { get; }
    public DateOnly VisitDate { get; }
    public decimal Price { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
}
