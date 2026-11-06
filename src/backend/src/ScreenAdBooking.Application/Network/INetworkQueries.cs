using ScreenAdBooking.Application.Contracts.Network;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Application.Network;

/// <param name="CountryCode">Upper-case ISO 3166-1 alpha-2 code.</param>
public sealed record VenueFilter(
    string? CountryCode,
    Guid? RegionId,
    Guid? CityId,
    VenueType? Type,
    Footfall? Footfall,
    LocationType? LocationType);

public interface INetworkQueries
{
    /// <returns>Countries, regions and cities sorted by name.</returns>
    Task<IReadOnlyList<NetworkCountry>> GetLocationsAsync(CancellationToken cancellationToken);

    /// <returns>Venues sorted by city then name.</returns>
    Task<(IReadOnlyList<VenueSummary> Items, int TotalCount)> SearchVenuesAsync(
        VenueFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken);
}
