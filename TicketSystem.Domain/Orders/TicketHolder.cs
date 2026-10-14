namespace TicketSystem.Domain.Orders;

public sealed class TicketHolder
{
    public TicketHolder(string firstName, string lastName, string email)
    {
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

        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public string FirstName { get; }

    public string LastName { get; }

    public string Email { get; }
}
