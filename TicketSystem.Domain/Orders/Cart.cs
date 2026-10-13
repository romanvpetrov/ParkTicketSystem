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
        var existingItem = _items.FirstOrDefault(cartItem =>
            cartItem.ProductId == item.ProductId &&
            cartItem.VisitDate == item.VisitDate);

        if (existingItem is not null)
        {
            var existingItemIndex = _items.IndexOf(existingItem);
            _items[existingItemIndex] = existingItem.IncreaseQuantityBy(item.Quantity);
            return;
        }

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
}
