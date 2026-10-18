namespace TicketSystem.Domain.Parks;

public enum RecordTicketsSoldFailure
{
    None,
    ProductNotFound,
    InvalidQuantity,
    NotEnoughInventory
}

public sealed record RecordTicketsSoldResult(
    bool Success,
    RecordTicketsSoldFailure Failure,
    string? ProductId,
    DateOnly? VisitDate,
    int RequestedItemCount,
    int AvailableItemCount)
{
    public static RecordTicketsSoldResult Succeeded()
    {
        return new RecordTicketsSoldResult(
            true,
            RecordTicketsSoldFailure.None,
            null,
            null,
            0,
            0);
    }

    public static RecordTicketsSoldResult Failed(
        RecordTicketsSoldFailure failure,
        string productId,
        DateOnly visitDate,
        int requestedItemCount,
        int availableItemCount)
    {
        return new RecordTicketsSoldResult(
            false,
            failure,
            productId,
            visitDate,
            requestedItemCount,
            availableItemCount);
    }
}
