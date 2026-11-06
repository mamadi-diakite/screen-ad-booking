using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class ScreenAdBookingDbContextTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Saves_and_reads_a_venue_with_its_place()
    {
        var city = await AddCityAsync();
        var venue = new Venue(
            city.Id, "Fresh Market 12", VenueType.Supermarket, Footfall.High, LocationType.CityCentre,
            "4 Example Square", 48.111, -1.68, 3, 40, Now);

        await using (var db = _database.CreateContext())
        {
            db.Venues.Add(venue);
            await db.SaveChangesAsync();
        }

        await using (var db = _database.CreateContext())
        {
            var loaded = await db.Venues
                .Include(v => v.City!).ThenInclude(c => c.Region!).ThenInclude(r => r.Country)
                .SingleAsync();

            Assert.Equal("Fresh Market 12", loaded.Name);
            Assert.Equal(VenueType.Supermarket, loaded.Type);
            Assert.Equal(Footfall.High, loaded.Footfall);
            Assert.Equal(LocationType.CityCentre, loaded.LocationType);
            Assert.Equal(48.111, loaded.Latitude);
            Assert.Equal("Rennes", loaded.City!.Name);
            Assert.Equal("Brittany", loaded.City.Region!.Name);
            Assert.Equal("FR", loaded.City.Region.Country!.Code);
            Assert.Equal(DateTimeKind.Utc, loaded.CreatedAtUtc.Kind);
        }
    }

    [Fact]
    public async Task Saves_and_reads_users_with_their_company()
    {
        var company = new Company("Fresh Juice Co", "Food", Now);
        var advertiser = new User("anna@example.com", "Anna", "hash", UserRole.Advertiser, company.Id, Now);
        var admin = new User("admin@example.com", "Admin", "hash", UserRole.AgencyAdmin, null, Now);

        await using (var db = _database.CreateContext())
        {
            db.Companies.Add(company);
            db.Users.AddRange(advertiser, admin);
            await db.SaveChangesAsync();
        }

        await using (var db = _database.CreateContext())
        {
            var users = await db.Users.Include(u => u.Company).OrderBy(u => u.Email).ToListAsync();

            Assert.Equal(UserRole.AgencyAdmin, users[0].Role);
            Assert.Null(users[0].Company);
            Assert.Equal(UserRole.Advertiser, users[1].Role);
            Assert.Equal("Fresh Juice Co", users[1].Company!.Name);
        }
    }

    [Fact]
    public async Task Email_is_unique()
    {
        await using var db = _database.CreateContext();
        db.Users.Add(new User("admin@example.com", "Admin", "hash", UserRole.AgencyAdmin, null, Now));
        db.Users.Add(new User("Admin@Example.com", "Admin 2", "hash", UserRole.AgencyAdmin, null, Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Country_code_is_unique()
    {
        await using var db = _database.CreateContext();
        db.Countries.Add(new Country("FR", "France"));
        db.Countries.Add(new Country("fr", "France again"));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task A_city_with_venues_cannot_be_deleted()
    {
        var city = await AddCityAsync();
        await using (var db = _database.CreateContext())
        {
            db.Venues.Add(new Venue(city.Id, "Fresh Market 12", VenueType.Supermarket, Footfall.High,
                LocationType.CityCentre, "4 Example Square", 48.1, -1.6, 1, 10, Now));
            await db.SaveChangesAsync();
        }

        await using var context = _database.CreateContext();
        context.Cities.Remove(await context.Cities.SingleAsync());

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private async Task<City> AddCityAsync()
    {
        var country = new Country("FR", "France");
        var region = new Region(country.Id, "Brittany");
        var city = new City(region.Id, "Rennes", 48.1173, -1.6778);

        await using var db = _database.CreateContext();
        db.Countries.Add(country);
        db.Regions.Add(region);
        db.Cities.Add(city);
        await db.SaveChangesAsync();

        return city;
    }
}
