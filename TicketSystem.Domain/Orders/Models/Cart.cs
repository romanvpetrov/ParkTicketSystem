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

    public bool IsPaid { get; private set; }

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

    public CartPaymentResult MarkPaid()
    {
        if (IsPaid)
        {
            return CartPaymentResult.Failed("CartAlreadyPaid");
        }

        if (_items.Count == 0)
        {
            return CartPaymentResult.Failed("CartIsEmpty");
        }

        if (_items.Any(item => item.TicketHolder is null))
        {
            return CartPaymentResult.Failed("TicketHolderRequired");
        }

        var tickets = _items
            .SelectMany(item => Enumerable.Range(0, item.Quantity)
                .Select(_ => item.CreateTicket(Id)))
            .ToArray();

        IsPaid = true;

        return CartPaymentResult.Succeeded(tickets);
    }
}
