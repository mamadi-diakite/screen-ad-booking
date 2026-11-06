using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Application.Contracts.Network;
using ScreenAdBooking.Application.Network;

namespace ScreenAdBooking.Infrastructure.Persistence;

public sealed class NetworkQueries(ScreenAdBookingDbContext db) : INetworkQueries
{
    public async Task<IReadOnlyList<NetworkCountry>> GetLocationsAsync(CancellationToken cancellationToken)
    {
        var countries = await db.Countries.AsNoTracking().ToListAsync(cancellationToken);
        var regions = await db.Regions.AsNoTracking().ToListAsync(cancellationToken);
        var cities = await db.Cities.AsNoTracking().ToListAsync(cancellationToken);
        var byName = StringComparer.InvariantCultureIgnoreCase;

        return
        [
            .. countries.OrderBy(c => c.Name, byName).Select(country => new NetworkCountry(
                country.Code,
                country.Name,
                [
                    .. regions.Where(r => r.CountryId == country.Id).OrderBy(r => r.Name, byName).Select(region => new NetworkRegion(
                        region.Id,
                        region.Name,
                        [
                            .. cities.Where(c => c.RegionId == region.Id)
                                .OrderBy(c => c.Name, byName)
                                .Select(c => new NetworkCity(c.Id, c.Name, c.Latitude, c.Longitude)),
                        ])),
                ])),
        ];
    }

    public async Task<(IReadOnlyList<VenueSummary> Items, int TotalCount)> SearchVenuesAsync(
        VenueFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = db.Venues.AsNoTracking();
        if (filter.CountryCode is { } countryCode)
        {
            query = query.Where(v => v.City!.Region!.Country!.Code == countryCode);
        }

        if (filter.RegionId is { } regionId)
        {
            query = query.Where(v => v.City!.RegionId == regionId);
        }

        if (filter.CityId is { } cityId)
        {
            query = query.Where(v => v.CityId == cityId);
        }

        if (filter.Type is { } type)
        {
            query = query.Where(v => v.Type == type);
        }

        if (filter.Footfall is { } footfall)
        {
            query = query.Where(v => v.Footfall == footfall);
        }

        if (filter.LocationType is { } locationType)
        {
            query = query.Where(v => v.LocationType == locationType);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderBy(v => v.City!.Name)
            .ThenBy(v => v.Name)
            .ThenBy(v => v.Id)
            .Skip(skip)
            .Take(take)
            .Select(v => new
            {
                Venue = v,
                CountryCode = v.City!.Region!.Country!.Code,
                RegionName = v.City.Region.Name,
                CityName = v.City.Name,
            })
            .ToListAsync(cancellationToken);

        return (
            [
                .. rows.Select(r => new VenueSummary(
                    r.Venue.Id,
                    r.Venue.Name,
                    r.Venue.Type.ToString(),
                    r.Venue.Footfall.ToString(),
                    r.Venue.LocationType.ToString(),
                    r.Venue.Address,
                    r.Venue.Latitude,
                    r.Venue.Longitude,
                    r.Venue.ScreenCount,
                    r.Venue.AverageAudience,
                    r.CountryCode,
                    r.RegionName,
                    r.Venue.CityId,
                    r.CityName)),
            ],
            totalCount);
    }
}
