using TicketSystem.Domain.Orders;
using TicketSystem.Domain.Repositories;
using TicketSystem.Infrastructure.Persistence;

namespace TicketSystem.Infrastructure.Shopping;

public sealed class JsonTicketRepository : ITicketRepository
{
    private readonly JsonFileStore<List<TicketDocument>> _store;

    public JsonTicketRepository(string dataDirectory)
    {
        _store = new JsonFileStore<List<TicketDocument>>(dataDirectory, "tickets.json", () => []);
    }

    public Ticket? GetById(Guid ticketId)
    {
        return _store.Read(tickets =>
        {
            var document = tickets.SingleOrDefault(ticket => ticket.Id == ticketId);
            return document is null ? null : ToDomain(document);
        });
    }

    public void Save(Ticket ticket)
    {
        _store.Update(tickets =>
        {
            tickets.RemoveAll(existing => existing.Id == ticket.Id);
            tickets.Add(ToDocument(ticket));
        });
    }

    public void Commit()
    {
        _store.Commit();
    }

    private static Ticket ToDomain(TicketDocument ticket)
    {
        return new Ticket(
            ticket.Id,
            ticket.CartId,
            ticket.CartItemId,
            ticket.ProductId,
            ticket.ParkUniqueName,
            ticket.VisitDate,
            ticket.Price,
            ticket.FirstName,
            ticket.LastName,
            ticket.Email);
    }

    private static TicketDocument ToDocument(Ticket ticket)
    {
        return new TicketDocument(
            ticket.Id,
            ticket.CartId,
            ticket.CartItemId,
            ticket.ProductId,
            ticket.ParkUniqueName,
            ticket.VisitDate,
            ticket.Price,
            ticket.FirstName,
            ticket.LastName,
            ticket.Email);
    }

    private sealed record TicketDocument(
        Guid Id,
        Guid CartId,
        Guid CartItemId,
        string ProductId,
        string ParkUniqueName,
        DateOnly VisitDate,
        decimal Price,
        string FirstName,
        string LastName,
        string Email);
}
