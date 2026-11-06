using ScreenAdBooking.Application.Auth;
using ScreenAdBooking.Application.Contracts.Network;
using ScreenAdBooking.Application.Network;
using ScreenAdBooking.Application.Security;
using ScreenAdBooking.Application.Users;
using ScreenAdBooking.Domain;

namespace ScreenAdBooking.Application.Tests;

internal sealed class InMemoryUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public int SaveCount { get; private set; }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(u => u.Email == email));

    public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Users.SingleOrDefault(u => u.Id == id));

    public List<Company> Companies { get; } = [];

    public void Add(User user) => Users.Add(user);

    public void AddCompany(Company company) => Companies.Add(company);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

/// <summary>Readable fake: the hash is the password with a prefix.</summary>
internal sealed class FakePasswordHasher : IPasswordHasher
{
    public int HashCount { get; private set; }

    public string Hash(string password)
    {
        HashCount++;
        return "hashed:" + password;
    }

    public bool Verify(string passwordHash, string password) => passwordHash == "hashed:" + password;
}

internal sealed class FakeTokenIssuer : IAccessTokenIssuer
{
    public static readonly DateTime ExpiresAt = new(2026, 11, 6, 10, 0, 0, DateTimeKind.Utc);

    public AccessToken Issue(User user) => new($"token-for-{user.Id}", ExpiresAt);
}

internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}

internal sealed class FakeNetworkQueries : INetworkQueries
{
    public IReadOnlyList<NetworkCountry> Locations { get; set; } = [];

    public IReadOnlyList<VenueSummary> Venues { get; set; } = [];

    public (VenueFilter Filter, int Skip, int Take)? LastSearch { get; private set; }

    public Task<IReadOnlyList<NetworkCountry>> GetLocationsAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Locations);

    public Task<(IReadOnlyList<VenueSummary> Items, int TotalCount)> SearchVenuesAsync(
        VenueFilter filter,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        LastSearch = (filter, skip, take);

        return Task.FromResult<(IReadOnlyList<VenueSummary>, int)>(([.. Venues.Skip(skip).Take(take)], Venues.Count));
    }
}
