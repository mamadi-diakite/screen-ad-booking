using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Application.Network;
using ScreenAdBooking.Domain;
using ScreenAdBooking.Infrastructure.Persistence;
using ScreenAdBooking.Infrastructure.Security;
using ScreenAdBooking.Infrastructure.Seeding;

namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class NetworkQueriesTests : IDisposable
{
    private static readonly VenueFilter NoFilter = new(null, null, null, null, null, null);

    private readonly SqliteTestDatabase _database = new();

    public NetworkQueriesTests()
    {
        using var db = _database.CreateContext();
        new DemoDataSeeder(db, new IdentityPasswordHasher(), new FixedTimeProvider(new DateTime(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc)))
            .SeedAsync().GetAwaiter().GetResult();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Lists_countries_regions_and_cities_by_name()
    {
        await using var db = _database.CreateContext();

        var locations = await new NetworkQueries(db).GetLocationsAsync(default);

        Assert.Equal(["Belgium", "France"], locations.Select(c => c.Name));
        Assert.Equal(["Brussels-Capital Region", "Flanders", "Wallonia"], locations[0].Regions.Select(r => r.Name));
        Assert.Equal(["Antwerp", "Ghent"], locations[0].Regions[1].Cities.Select(c => c.Name));
        Assert.Equal(
            ["Auvergne-Rhône-Alpes", "Île-de-France", "Occitanie", "Provence-Alpes-Côte d'Azur"],
            locations[1].Regions.Select(r => r.Name));
        Assert.Equal(DemoNetwork.Cities.Count, locations.Sum(c => c.Regions.Sum(r => r.Cities.Count)));
    }

    [Fact]
    public async Task Pages_through_every_venue_by_city_then_name()
    {
        await using var db = _database.CreateContext();
        var queries = new NetworkQueries(db);

        var (first, total) = await queries.SearchVenuesAsync(NoFilter, 0, 3, default);
        var (second, _) = await queries.SearchVenuesAsync(NoFilter, 3, 3, default);

        Assert.Equal(DemoNetwork.Venues.Count, total);
        Assert.Equal(["Harbour Lights Shopping Centre", "Market Street Pharmacy", "Bridge Pharmacy"], first.Select(v => v.Name));
        Assert.Equal("Riverside Fitness", second[0].Name);
        Assert.Equal(("FR", "Île-de-France", "Boulogne-Billancourt"), (second[0].CountryCode, second[0].RegionName, second[0].CityName));
        Assert.Equal(("Gym", "Medium", "Suburb", 3, 110), (second[0].Type, second[0].Footfall, second[0].LocationType, second[0].ScreenCount, second[0].AverageAudience));
    }

    [Fact]
    public async Task Filters_by_country_type_footfall_and_location_type()
    {
        await using var db = _database.CreateContext();
        var queries = new NetworkQueries(db);

        var (belgianBakeries, _) = await queries.SearchVenuesAsync(NoFilter with { CountryCode = "BE", Type = VenueType.Bakery }, 0, 10, default);
        var (busyHubs, _) = await queries.SearchVenuesAsync(
            NoFilter with { Footfall = Footfall.High, LocationType = LocationType.TransportHub }, 0, 10, default);

        Assert.Equal(["Garden Square Bakery", "Rampart Bakery"], belgianBakeries.Select(v => v.Name));
        Assert.Equal(["Central Station Arcade", "Station Square Market", "Hilltop Station Hall", "North Station Concourse"], busyHubs.Select(v => v.Name));
    }

    [Fact]
    public async Task Filters_by_region_and_city()
    {
        await using var db = _database.CreateContext();
        var flanders = await db.Regions.SingleAsync(r => r.Name == "Flanders");
        var ghent = await db.Cities.SingleAsync(c => c.Name == "Ghent");
        var queries = new NetworkQueries(db);

        var (inRegion, regionTotal) = await queries.SearchVenuesAsync(NoFilter with { RegionId = flanders.Id }, 0, 10, default);
        var (inCity, _) = await queries.SearchVenuesAsync(NoFilter with { CityId = ghent.Id }, 0, 10, default);

        Assert.Equal(4, regionTotal);
        Assert.All(inRegion, v => Assert.Equal("Flanders", v.RegionName));
        Assert.All(inCity, v => Assert.Equal(ghent.Id, v.CityId));
        Assert.Equal(2, inCity.Count);
    }

    [Fact]
    public async Task Rejects_a_missing_filter()
    {
        await using var db = _database.CreateContext();

        await Assert.ThrowsAsync<ArgumentNullException>(() => new NetworkQueries(db).SearchVenuesAsync(null!, 0, 10, default));
    }
}
