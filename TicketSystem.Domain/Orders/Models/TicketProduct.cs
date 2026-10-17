namespace TicketSystem.Domain.Orders;

public sealed class TicketProduct
{
    public TicketProduct(string id, string parkUniqueName, DateOnly visitDate, decimal price)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Product id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(parkUniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(parkUniqueName));
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        Id = id;
        ParkUniqueName = parkUniqueName;
        VisitDate = visitDate;
        Price = price;
    }

    public string Id { get; }

    public string ParkUniqueName { get; }

    public DateOnly VisitDate { get; }

    public decimal Price { get; }
}
