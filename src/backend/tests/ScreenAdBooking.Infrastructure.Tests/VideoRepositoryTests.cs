namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class VideoRepositoryTests : IDisposable
{
    private static readonly DateTime Now = new(2026, 11, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly SqliteTestDatabase _database = new();
    private readonly Company _company = new("Lumen Bakery", null, Now);
    private readonly Company _other = new("Urban Cycle Shop", null, Now);

    public VideoRepositoryTests()
    {
        using var db = _database.CreateContext();
        db.Companies.AddRange(_company, _other);
        db.SaveChanges();
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task Saves_videos_and_lists_a_company_newest_first()
    {
        var older = new Video(_company.Id, "Morning bread", VideoFormat.Mp4, 1_024, Now);
        var newer = new Video(_company.Id, "Evening pastries", VideoFormat.WebM, 2_048, Now.AddHours(1));
        var foreign = new Video(_other.Id, "Bike repair", VideoFormat.Mp4, 512, Now);
        await using (var db = _database.CreateContext())
        {
            var repository = new VideoRepository(db);
            repository.Add(older);
            repository.Add(newer);
            repository.Add(foreign);
            await repository.SaveChangesAsync(default);
        }

        await using var check = _database.CreateContext();
        var repositoryCheck = new VideoRepository(check);
        var list = await repositoryCheck.ListAsync(_company.Id, default);

        Assert.Equal([newer.Id, older.Id], list.Select(v => v.Id));
        Assert.Equal((VideoFormat.WebM, 2_048L, VideoReviewStatus.PendingReview), (list[0].Format, list[0].SizeBytes, list[0].Status));
        Assert.Equal(DateTimeKind.Utc, list[0].UploadedAtUtc.Kind);
        Assert.Equal("Morning bread", (await repositoryCheck.FindAsync(_company.Id, older.Id, default))?.Name);
        Assert.Null(await repositoryCheck.FindAsync(_company.Id, foreign.Id, default));
    }

    [Fact]
    public async Task A_video_belongs_to_an_existing_company()
    {
        await using var db = _database.CreateContext();
        db.Videos.Add(new Video(Guid.NewGuid(), "Orphan", VideoFormat.Mp4, 1, Now));

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}
