using ScreenAdBooking.Application.Contracts.Network;
using ScreenAdBooking.Application.Network;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Application.Tests;

public sealed class VenueSearchServiceTests
{
    private readonly FakeNetworkQueries _queries = new();
    private readonly VenueSearchService _service;

    public VenueSearchServiceTests()
    {
        _service = new VenueSearchService(_queries);
    }

    [Fact]
    public async Task Returns_the_locations()
    {
        _queries.Locations = [new NetworkCountry("FR", "France", [])];

        Assert.Same(_queries.Locations, await _service.GetLocationsAsync());
    }

    [Fact]
    public async Task Without_filters_searches_every_venue_on_the_first_page()
    {
        _queries.Venues = [Venue("Canal Fresh Market"), Venue("Riviera Mall")];

        var result = await _service.SearchAsync(new VenueSearchRequest());

        Assert.Empty(result.Errors);
        Assert.Equal(2, result.Page?.Items.Count);
        Assert.Equal((1, 20, 2), (result.Page!.Page, result.Page.PageSize, result.Page.TotalCount));
        Assert.Equal((new VenueFilter(null, null, null, null, null, null), 0, 20), _queries.LastSearch);
    }

    [Fact]
    public async Task Reads_every_filter_ignoring_case_and_spaces()
    {
        var regionId = Guid.NewGuid();
        var cityId = Guid.NewGuid();

        await _service.SearchAsync(new VenueSearchRequest(" be ", regionId, cityId, "bakery", " HIGH ", "cityCentre", Page: 3, PageSize: 5));

        Assert.Equal(
            (new VenueFilter("BE", regionId, cityId, VenueType.Bakery, Footfall.High, LocationType.CityCentre), 10, 5),
            _queries.LastSearch);
    }

    [Fact]
    public async Task Keeps_page_values_in_range_and_ignores_blank_filters()
    {
        await _service.SearchAsync(new VenueSearchRequest(" ", Type: "", Footfall: " ", Page: 0, PageSize: 1000));

        Assert.Equal(
            (new VenueFilter(null, null, null, null, null, null), 0, VenueSearchService.MaxPageSize),
            _queries.LastSearch);
    }

    [Theory]
    [InlineData("FRA")]
    [InlineData("F1")]
    public async Task Rejects_an_unreadable_country(string country)
    {
        var result = await _service.SearchAsync(new VenueSearchRequest(country));

        Assert.Null(result.Page);
        Assert.Equal(["Use a two-letter country code, for example FR."], result.Errors["country"]);
        Assert.Null(_queries.LastSearch);
    }

    [Fact]
    public async Task Reports_every_unknown_filter_value()
    {
        var result = await _service.SearchAsync(new VenueSearchRequest(Type: "Museum", Footfall: "2", LocationType: "Airport"));

        Assert.Equal(["Choose a venue type."], result.Errors["type"]);
        Assert.Equal(["Choose a footfall level."], result.Errors["footfall"]);
        Assert.Equal(["Choose a location type."], result.Errors["locationType"]);
        Assert.Null(_queries.LastSearch);
    }

    [Fact]
    public async Task Rejects_a_missing_request()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.SearchAsync(null!));
    }

    private static VenueSummary Venue(string name) =>
        new(Guid.NewGuid(), name, "Supermarket", "High", "CityCentre", "1 Example Street", 48.85, 2.35, 2, 100, "FR", "Île-de-France", Guid.NewGuid(), "Paris");
}
