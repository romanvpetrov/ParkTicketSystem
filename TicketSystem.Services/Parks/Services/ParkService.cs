using TicketSystem.Domain.Repositories;

namespace TicketSystem.Services.Parks;

public sealed class ParkService
{
    private readonly IParkRepository _parkRepository;

    public ParkService(IParkRepository parkRepository)
    {
        _parkRepository = parkRepository;
    }

    public IReadOnlyList<ParkDto> GetParks()
    {
        return _parkRepository.GetAll()
            .Select(park => new ParkDto(park.UniqueName, park.Name))
            .ToArray();
    }

    public IReadOnlyList<DailyTicketAvailabilityDto> GetMonthlyAvailability(string uniqueName, int year, int month)
    {
        var park = _parkRepository.GetByUniqueName(uniqueName);

        if (park is null)
        {
            return [];
        }

        return park.GetMonthlyInventory(year, month)
            .Select(dailyInventory => new DailyTicketAvailabilityDto(
                dailyInventory.ProductId,
                dailyInventory.Name,
                dailyInventory.Date,
                dailyInventory.AvailableItemCount,
                dailyInventory.Price))
            .ToArray();
    }
}
