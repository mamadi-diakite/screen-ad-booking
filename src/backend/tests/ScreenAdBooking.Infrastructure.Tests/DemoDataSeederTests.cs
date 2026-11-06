using Microsoft.EntityFrameworkCore;
using ScreenAdBooking.Domain;
using ScreenAdBooking.Infrastructure.Security;
using ScreenAdBooking.Infrastructure.Seeding;

namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class DemoDataSeederTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly IdentityPasswordHasher _hasher = new();

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Seeds_one_agency_admin_and_two_advertisers_in_their_companies()
    {
        Assert.True(await SeedAsync());

        await using var db = _database.CreateContext();
        var users = await db.Users.Include(u => u.Company).ToListAsync();

        var admin = Assert.Single(users, u => u.Role == UserRole.AgencyAdmin);
        Assert.Equal(DemoDataSeeder.AgencyAdminEmail, admin.Email);
        Assert.Null(admin.CompanyId);

        var advertisers = users.Where(u => u.Role == UserRole.Advertiser).OrderBy(u => u.Email).ToList();
        Assert.Equal(DemoDataSeeder.Advertisers.Select(a => a.Email).Order(), advertisers.Select(u => u.Email));
        Assert.All(advertisers, u => Assert.NotNull(u.Company));
        Assert.Equal(2, await db.Companies.CountAsync());
        Assert.All(users, u => Assert.EndsWith("@example.com", u.Email));
        Assert.All(users, u => Assert.Equal(Now, u.CreatedAtUtc));
    }

    [Fact]
    public async Task Every_demo_account_accepts_the_demo_password()
    {
        await SeedAsync();

        await using var db = _database.CreateContext();
        foreach (var user in await db.Users.ToListAsync())
        {
            Assert.True(_hasher.Verify(user.PasswordHash, DemoDataSeeder.DemoPassword), user.Email);
        }
    }

    [Fact]
    public async Task Seeds_the_demo_network_in_france_and_belgium()
    {
        await SeedAsync();

        await using var db = _database.CreateContext();
        var venues = await db.Venues.Include(v => v.City!).ThenInclude(c => c.Region!).ThenInclude(r => r.Country).ToListAsync();

        Assert.Equal(["BE", "FR"], (await db.Countries.Select(c => c.Code).ToListAsync()).Order());
        Assert.Equal(DemoNetwork.Cities.Count, await db.Cities.CountAsync());
        Assert.Equal(DemoNetwork.Venues.Count, venues.Count);
        foreach (var country in new[] { "FR", "BE" })
        {
            var inCountry = venues.Where(v => v.City!.Region!.Country!.Code == country).ToList();
            Assert.True(inCountry.Select(v => v.City!.RegionId).Distinct().Count() >= 3, country);
            Assert.True(inCountry.Select(v => v.CityId).Distinct().Count() >= 5, country);
        }

        Assert.All(venues, v => Assert.InRange(Math.Abs(v.Latitude - v.City!.Latitude), 0, 0.05));
        Assert.All(venues, v => Assert.Equal(Now, v.CreatedAtUtc));
        Assert.Equal(Enum.GetValues<VenueType>(), venues.Select(v => v.Type).Distinct().Order());
    }

    [Fact]
    public async Task Second_run_adds_nothing()
    {
        await SeedAsync();

        Assert.False(await SeedAsync());

        await using var db = _database.CreateContext();
        Assert.Equal(3, await db.Users.CountAsync());
        Assert.Equal(2, await db.Companies.CountAsync());
        Assert.Equal(DemoNetwork.Venues.Count, await db.Venues.CountAsync());
    }

    [Fact]
    public async Task Skips_a_database_that_already_has_users()
    {
        await using (var db = _database.CreateContext())
        {
            db.Users.Add(new User("someone@example.com", "Someone", "hash", UserRole.AgencyAdmin, null, Now));
            await db.SaveChangesAsync();
        }

        Assert.False(await SeedAsync());

        await using var check = _database.CreateContext();
        Assert.Equal(1, await check.Users.CountAsync());
        Assert.Equal(0, await check.Companies.CountAsync());
    }

    private async Task<bool> SeedAsync()
    {
        await using var db = _database.CreateContext();

        return await new DemoDataSeeder(db, _hasher, new FixedTimeProvider(Now)).SeedAsync();
    }
}
