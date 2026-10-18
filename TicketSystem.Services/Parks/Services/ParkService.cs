using TicketSystem.Domain.Repositories;

namespace TicketSystem.Services.Parks;

public sealed class ParkService
{
    private readonly IParkRepository _parkRepository;
    private readonly TimeProvider _timeProvider;

    public ParkService(IParkRepository parkRepository)
        : this(parkRepository, TimeProvider.System)
    {
    }

    public ParkService(IParkRepository parkRepository, TimeProvider timeProvider)
    {
        _parkRepository = parkRepository;
        _timeProvider = timeProvider;
    }

    public IReadOnlyList<ParkDto> GetParks()
    {
        return _parkRepository.GetAll()
            .Select(park => new ParkDto(
                park.UniqueName,
                park.Name,
                park.Location,
                park.TagText,
                park.TimeZoneId))
            .ToArray();
    }

    public IReadOnlyList<DailyTicketAvailabilityDto> GetMonthlyAvailability(string uniqueName, int year, int month)
    {
        var park = _parkRepository.GetByUniqueName(uniqueName);

        if (park is null)
        {
            return [];
        }

        return park.GetMonthlyInventory(year, month, _timeProvider.GetUtcNow())
            .Select(dailyInventory => new DailyTicketAvailabilityDto(
                dailyInventory.ProductId,
                dailyInventory.Name,
                dailyInventory.Date,
                dailyInventory.AvailableItemCount,
                dailyInventory.Price))
            .ToArray();
    }
}
