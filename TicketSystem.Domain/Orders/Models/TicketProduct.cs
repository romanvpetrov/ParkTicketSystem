namespace TicketSystem.Domain.Orders;

public sealed class TicketProduct
{
    public TicketProduct(
        string id,
        string parkUniqueName,
        string name,
        int defaultAvailableTicketCount,
        decimal price)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Product id is required.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(parkUniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(parkUniqueName));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name is required.", nameof(name));
        }

        if (defaultAvailableTicketCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultAvailableTicketCount),
                "Default available ticket count cannot be negative.");
        }

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        }

        Id = id;
        ParkUniqueName = parkUniqueName;
        Name = name;
        DefaultAvailableTicketCount = defaultAvailableTicketCount;
        Price = price;
    }

    public string Id { get; }

    public string ParkUniqueName { get; }

    public string Name { get; }

    public int DefaultAvailableTicketCount { get; }

    public decimal Price { get; }

    public Parks.DailyInventory CreateDailyInventory(DateOnly visitDate)
    {
        return new Parks.DailyInventory(
            Id,
            ParkUniqueName,
            Name,
            visitDate,
            DefaultAvailableTicketCount,
            0,
            Price);
    }
}
