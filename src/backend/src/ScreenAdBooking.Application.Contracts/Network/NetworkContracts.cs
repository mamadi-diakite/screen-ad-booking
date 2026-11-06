namespace ScreenAdBooking.Application.Contracts.Network;

public sealed record NetworkCountry(string Code, string Name, IReadOnlyList<NetworkRegion> Regions);

public sealed record NetworkRegion(Guid Id, string Name, IReadOnlyList<NetworkCity> Cities);

public sealed record NetworkCity(Guid Id, string Name, double Latitude, double Longitude);

/// <summary>Every filter is optional. Enum filters take the value name, e.g. <c>Bakery</c>.</summary>
/// <param name="Country">ISO 3166-1 alpha-2 code, e.g. FR.</param>
public sealed record VenueSearchRequest(
    string? Country = null,
    Guid? RegionId = null,
    Guid? CityId = null,
    string? Type = null,
    string? Footfall = null,
    string? LocationType = null,
    int Page = 1,
    int PageSize = 20);

public sealed record VenueSummary(
    Guid Id,
    string Name,
    string Type,
    string Footfall,
    string LocationType,
    string Address,
    double Latitude,
    double Longitude,
    int ScreenCount,
    int AverageAudience,
    string CountryCode,
    string RegionName,
    Guid CityId,
    string CityName);

public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
