using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Application.Security;
using ScreenAdBooking.Domain;
using ScreenAdBooking.Infrastructure.Persistence;

namespace ScreenAdBooking.Infrastructure.Seeding;

public sealed class DemoDataSeeder(ScreenAdBookingDbContext db, IPasswordHasher passwordHasher, TimeProvider timeProvider)
{
    public const string DemoPassword = "TestDemo123!";
    public const string AgencyAdminEmail = "nora.vidal@example.com";

    /// <summary>Fictional advertisers, each in its own company. All names are made up.</summary>
    public static IReadOnlyList<(string Email, string DisplayName, string CompanyName, string Sector)> Advertisers { get; } =
    [
        ("paul.mercier@example.com", "Paul Mercier", "Lumen Bakery", "Food and drink"),
        ("lina.chevalier@example.com", "Lina Chevalier", "Urban Cycle Shop", "Sports retail"),
    ];

    /// <summary>Seeds the demo accounts and the <see cref="DemoNetwork"/>.</summary>
    /// <returns><c>false</c> when the database already holds users.</returns>
    public async Task<bool> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(cancellationToken))
        {
            return false;
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var passwordHash = passwordHasher.Hash(DemoPassword);

        db.Users.Add(new User(AgencyAdminEmail, "Nora Vidal", passwordHash, UserRole.AgencyAdmin, companyId: null, now));

        foreach (var advertiser in Advertisers)
        {
            var company = new Company(advertiser.CompanyName, advertiser.Sector, now);
            db.Companies.Add(company);
            db.Users.Add(new User(
                advertiser.Email, advertiser.DisplayName, passwordHash, UserRole.Advertiser, company.Id, now));
        }

        AddNetwork(now);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    private void AddNetwork(DateTime now)
    {
        var countries = DemoNetwork.Countries.ToDictionary(c => c.Code, c => new Country(c.Code, c.Name));
        db.Countries.AddRange(countries.Values);

        var regions = new Dictionary<string, Region>();
        var cities = new Dictionary<string, City>();
        foreach (var demoCity in DemoNetwork.Cities)
        {
            if (!regions.TryGetValue(demoCity.Region, out var region))
            {
                region = new Region(countries[demoCity.CountryCode].Id, demoCity.Region);
                regions.Add(demoCity.Region, region);
                db.Regions.Add(region);
            }

            var city = new City(region.Id, demoCity.Name, demoCity.Latitude, demoCity.Longitude);
            cities.Add(demoCity.Name, city);
            db.Cities.Add(city);
        }

        foreach (var venue in DemoNetwork.Venues)
        {
            var city = cities[venue.City];
            db.Venues.Add(new Venue(
                city.Id,
                venue.Name,
                venue.Type,
                venue.Footfall,
                venue.LocationType,
                venue.Address,
                city.Latitude + venue.LatitudeOffset,
                city.Longitude + venue.LongitudeOffset,
                venue.ScreenCount,
                venue.AverageAudience,
                now));
        }
    }
}
