using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScreenAdBooking.Application.Contracts.Network;
using ScreenAdBooking.Application.Network;

namespace ScreenAdBooking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class NetworkController(VenueSearchService venueSearchService) : ControllerBase
{
    /// <summary>Countries, regions and cities, to fill the location filters.</summary>
    [HttpGet("network/locations")]
    public async Task<ActionResult<IReadOnlyList<NetworkCountry>>> Locations(CancellationToken cancellationToken) =>
        Ok(await venueSearchService.GetLocationsAsync(cancellationToken));

    [HttpGet("venues")]
    public async Task<ActionResult<PagedResponse<VenueSummary>>> SearchVenues(
        string? country,
        Guid? regionId,
        Guid? cityId,
        string? type,
        string? footfall,
        string? locationType,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var request = new VenueSearchRequest(country, regionId, cityId, type, footfall, locationType, page, pageSize);
        var result = await venueSearchService.SearchAsync(request, cancellationToken);

        return result.Page is { } venues ? Ok(venues) : this.InvalidRequest(result.Errors);
    }
}
