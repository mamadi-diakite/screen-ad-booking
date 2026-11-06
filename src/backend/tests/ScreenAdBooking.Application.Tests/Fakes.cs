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

internal sealed class InMemoryCreditRepository : ICreditRepository
{
    public List<CreditMovement> Movements { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<CreditMovement>> ListAsync(Guid companyId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CreditMovement>>(
            [.. Movements.Where(m => m.CompanyId == companyId).OrderByDescending(m => m.CreatedAtUtc)]);

    public void Add(CreditMovement movement) => Movements.Add(movement);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryVideoRepository : IVideoRepository
{
    public List<Video> Videos { get; } = [];

    public int SaveCount { get; private set; }

    public Task<IReadOnlyList<Video>> ListAsync(Guid companyId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Video>>(
            [.. Videos.Where(v => v.CompanyId == companyId).OrderByDescending(v => v.UploadedAtUtc)]);

    public Task<Video?> FindAsync(Guid companyId, Guid videoId, CancellationToken cancellationToken) =>
        Task.FromResult(Videos.SingleOrDefault(v => v.Id == videoId && v.CompanyId == companyId));

    public void Add(Video video) => Videos.Add(video);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryVideoStorage : IVideoStorage
{
    public Dictionary<string, byte[]> Files { get; } = [];

    public async Task SaveAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);
        Files[fileName] = copy.ToArray();
    }

    public Stream? OpenRead(string fileName) =>
        Files.TryGetValue(fileName, out var bytes) ? new MemoryStream(bytes, writable: false) : null;
}

/// <summary>Smallest headers that pass or fail the video type check.</summary>
internal static class VideoBytes
{
    public static byte[] Mp4(int length = 64) => Padded([0, 0, 0, 0x18, .. "ftypisom"u8], length);

    public static byte[] QuickTime() => Padded([0, 0, 0, 0x14, .. "ftypqt  "u8], 64);

    public static byte[] WebM(int length = 64) => Padded([0x1A, 0x45, 0xDF, 0xA3, 0x9F, 0x42, 0x82, 0x84, .. "webm"u8], length);

    public static byte[] Matroska() => Padded([0x1A, 0x45, 0xDF, 0xA3, 0x9F, 0x42, 0x82, 0x88, .. "matroska"u8], 64);

    private static byte[] Padded(byte[] header, int length)
    {
        var bytes = new byte[Math.Max(length, header.Length)];
        header.CopyTo(bytes, 0);
        return bytes;
    }
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
