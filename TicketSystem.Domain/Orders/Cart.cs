namespace TicketSystem.Domain.Orders;

public sealed class Cart
{
    private readonly List<CartItem> _items = [];

    public Cart(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Cart id is required.", nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; }

    public IReadOnlyList<CartItem> Items => _items;

    public void AddItem(CartItem item)
    {
        _items.Add(item);
    }

    public bool RemoveItem(string productId, DateOnly visitDate)
    {
        var item = _items.FirstOrDefault(item =>
            item.ProductId == productId &&
            item.VisitDate == visitDate);

        if (item is null)
        {
            return false;
        }

        _items.Remove(item);

        return true;
    }

    public bool AssignTicketHolder(Guid cartItemId, TicketHolder ticketHolder)
    {
        var item = _items.FirstOrDefault(item => item.Id == cartItemId);

        if (item is null)
        {
            return false;
        }

        item.AssignTicketHolder(ticketHolder);

        return true;
    }
}
