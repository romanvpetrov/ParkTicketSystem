using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;
using TicketSystem.Infrastructure.Persistence;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class JsonCartRepository : ICartRepository
{
    private readonly JsonFileStore<List<CartDocument>> _store;

    public JsonCartRepository(string dataDirectory)
    {
        _store = new JsonFileStore<List<CartDocument>>(dataDirectory, "carts.json", () => []);
    }

    public Cart? GetById(Guid cartId)
    {
        return _store.Read(carts =>
        {
            var document = carts.SingleOrDefault(cart => cart.Id == cartId);
            return document is null ? null : ToDomain(document);
        });
    }

    public void Save(Cart cart)
    {
        _store.Update(carts =>
        {
            carts.RemoveAll(existing => existing.Id == cart.Id);
            carts.Add(ToDocument(cart));
        });
    }

    private static Cart ToDomain(CartDocument cart)
    {
        var items = cart.Items.Select(item =>
        {
            var cartItem = new CartItem(
                item.Id,
                item.ProductId,
                item.ParkUniqueName,
                item.VisitDate,
                item.Quantity,
                item.Price);

            if (item.TicketHolder is not null)
            {
                cartItem.AssignTicketHolder(new TicketHolder(
                    item.TicketHolder.FirstName,
                    item.TicketHolder.LastName,
                    item.TicketHolder.Email));
            }

            return cartItem;
        });

        return new Cart(cart.Id, items, cart.IsPaid);
    }

    private static CartDocument ToDocument(Cart cart)
    {
        return new CartDocument(
            cart.Id,
            cart.IsPaid,
            cart.Items.Select(item => new CartItemDocument(
                item.Id,
                item.ProductId,
                item.ParkUniqueName,
                item.VisitDate,
                item.Quantity,
                item.Price,
                item.TicketHolder is null
                    ? null
                    : new TicketHolderDocument(
                        item.TicketHolder.FirstName,
                        item.TicketHolder.LastName,
                        item.TicketHolder.Email))).ToList());
    }

    private sealed record CartDocument(Guid Id, bool IsPaid, List<CartItemDocument> Items);

    private sealed record CartItemDocument(
        Guid Id,
        string ProductId,
        string ParkUniqueName,
        DateOnly VisitDate,
        int Quantity,
        decimal Price,
        TicketHolderDocument? TicketHolder);

    private sealed record TicketHolderDocument(string FirstName, string LastName, string Email);
}
