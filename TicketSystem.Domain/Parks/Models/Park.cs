using TicketSystem.Domain.Orders;

namespace TicketSystem.Domain.Parks;

public sealed class Park
{
    public const string DefaultTimeZoneId = "America/New_York";

    public Park(int id, string uniqueName, string name, ParkCalendar calendar)
        : this(id, uniqueName, name, null, null, null, calendar)
    {
    }

    public Park(
        int id,
        string uniqueName,
        string name,
        string? location,
        string? tagText,
        ParkCalendar calendar)
        : this(id, uniqueName, name, location, tagText, null, calendar)
    {
    }

    public Park(
        int id,
        string uniqueName,
        string name,
        string? location,
        string? tagText,
        string? timeZoneId,
        ParkCalendar calendar)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            throw new ArgumentException("Park unique name is required.", nameof(uniqueName));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Park name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(calendar);

        if (calendar.DailyInventories.Any(inventory => inventory.ParkUniqueName != uniqueName) ||
            calendar.TicketProducts.Any(product => product.ParkUniqueName != uniqueName))
        {
            throw new ArgumentException("The park calendar contains products or inventory for another park.", nameof(calendar));
        }

        Id = id;
        UniqueName = uniqueName;
        Name = name;
        Location = string.IsNullOrWhiteSpace(location) ? null : location;
        TagText = string.IsNullOrWhiteSpace(tagText) ? null : tagText;
        TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? DefaultTimeZoneId : timeZoneId;
        Calendar = calendar;
    }

    public int Id { get; }

    public string UniqueName { get; }

    public string Name { get; }

    public string? Location { get; }

    public string? TagText { get; }

    public string TimeZoneId { get; }

    public ParkCalendar Calendar { get; }

    public IReadOnlyList<DailyInventory> GetMonthlyInventory(int year, int month)
    {
        return Calendar.GetMonthlyInventory(year, month);
    }

    public IReadOnlyList<DailyInventory> GetMonthlyInventory(int year, int month, DateTimeOffset utcNow)
    {
        return Calendar.GetMonthlyInventory(year, month, GetCurrentDate(utcNow));
    }

    public DateOnly GetCurrentDate(DateTimeOffset utcNow)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        return DateOnly.FromDateTime(localNow.DateTime);
    }

    public bool IsVisitDateInPast(CartItem cartItem, DateTimeOffset utcNow)
    {
        ArgumentNullException.ThrowIfNull(cartItem);

        if (cartItem.ParkUniqueName != UniqueName)
        {
            throw new ArgumentException("The cart item belongs to another park.", nameof(cartItem));
        }

        return Calendar.IsVisitDateInPast(cartItem, GetCurrentDate(utcNow));
    }

    public DailyInventory? GetDailyInventory(string productId, DateOnly date)
    {
        return Calendar.GetDailyInventory(productId, date);
    }

    public CartItemCreationResult TryCreateCartItem(string productId, DateOnly date, int quantity)
    {
        var inventory = Calendar.GetDailyInventory(productId, date);

        return inventory is null
            ? CartItemCreationResult.Failed(CartItemCreationFailure.ProductNotFound)
            : inventory.TryCreateCartItem(quantity);
    }

    public RecordTicketsSoldResult RecordTicketsSold(IReadOnlyList<Ticket> tickets)
    {
        var ticketsForPark = tickets
            .Where(ticket => ticket.ParkUniqueName == UniqueName)
            .ToArray();

        return Calendar.RecordTicketsSold(ticketsForPark);
    }
}
