using ScreenAdBooking.Application.Contracts.Network;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Application.Network;

public sealed class VenueSearchService(INetworkQueries queries)
{
    public const int MaxPageSize = 100;

    public Task<IReadOnlyList<NetworkCountry>> GetLocationsAsync(CancellationToken cancellationToken = default) =>
        queries.GetLocationsAsync(cancellationToken);

    /// <returns>The page of venues, or the errors of the filters that cannot be read.</returns>
    public async Task<VenueSearchResult> SearchAsync(VenueSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var errors = new Dictionary<string, string[]>();
        var country = request.Country?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(country))
        {
            country = null;
        }
        else if (country.Length != Country.CodeLength || !country.All(char.IsAsciiLetterUpper))
        {
            errors["country"] = ["Use a two-letter country code, for example FR."];
        }

        var type = ParseName<VenueType>(request.Type, "type", "Choose a venue type.", errors);
        var footfall = ParseName<Footfall>(request.Footfall, "footfall", "Choose a footfall level.", errors);
        var locationType = ParseName<LocationType>(request.LocationType, "locationType", "Choose a location type.", errors);
        if (errors.Count > 0)
        {
            return new VenueSearchResult(null, errors);
        }

        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var filter = new VenueFilter(country, request.RegionId, request.CityId, type, footfall, locationType);
        var (items, totalCount) = await queries.SearchVenuesAsync(filter, (page - 1) * pageSize, pageSize, cancellationToken);

        return new VenueSearchResult(new PagedResponse<VenueSummary>(items, page, pageSize, totalCount), errors);
    }

    private static TEnum? ParseName<TEnum>(string? value, string field, string message, Dictionary<string, string[]> errors)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var name = Enum.GetNames<TEnum>().FirstOrDefault(n => string.Equals(n, value.Trim(), StringComparison.OrdinalIgnoreCase));
        if (name is null)
        {
            errors[field] = [message];
            return null;
        }

        return Enum.Parse<TEnum>(name);
    }
}

/// <param name="Page">Null when <paramref name="Errors"/> has messages, by filter name in camelCase.</param>
public sealed record VenueSearchResult(PagedResponse<VenueSummary>? Page, IReadOnlyDictionary<string, string[]> Errors);
