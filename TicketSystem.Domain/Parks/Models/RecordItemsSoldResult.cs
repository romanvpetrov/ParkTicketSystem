namespace TicketSystem.Domain.Parks;

public enum RecordItemsSoldFailure
{
    None,
    InvalidQuantity,
    NotEnoughInventory
}

public sealed record RecordItemsSoldResult(
    bool Success,
    RecordItemsSoldFailure Failure,
    int AvailableItemCount)
{
    public static RecordItemsSoldResult Succeeded(int availableItemCount)
    {
        return new RecordItemsSoldResult(
            true,
            RecordItemsSoldFailure.None,
            availableItemCount);
    }

    public static RecordItemsSoldResult Failed(
        RecordItemsSoldFailure failure,
        int availableItemCount)
    {
        return new RecordItemsSoldResult(false, failure, availableItemCount);
    }
}
