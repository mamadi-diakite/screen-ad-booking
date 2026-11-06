namespace ScreenAdBooking.Application.Tests;

public sealed class VideoServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private readonly InMemoryUserRepository users = new();
    private readonly InMemoryVideoRepository videos = new();
    private readonly InMemoryVideoStorage storage = new();
    private readonly User advertiser;
    private readonly User admin;

    public VideoServiceTests()
    {
        var company = new Company("Fresh Juice Co", null, Now.UtcDateTime);
        advertiser = new User("ada@example.com", "Ada", "hash", UserRole.Advertiser, company.Id, Now.UtcDateTime);
        admin = new User("admin@example.com", "Admin", "hash", UserRole.AgencyAdmin, null, Now.UtcDateTime);
        users.Add(advertiser);
        users.Add(admin);
    }

    [Theory]
    [InlineData("spring-sale.mp4", "spring-sale", "Mp4")]
    [InlineData(" Summer promo .WEBM", "Summer promo", "WebM")]
    [InlineData(".mp4", VideoService.UntitledName, "Mp4")]
    public async Task Uploads_a_video_that_waits_for_review(string fileName, string name, string format)
    {
        var bytes = format == "Mp4" ? VideoBytes.Mp4(2048) : VideoBytes.WebM(2048);

        var result = await Upload(fileName, bytes);

        Assert.Equal(VideoStatus.Ok, result.Status);
        Assert.Empty(result.Errors);
        var video = Assert.Single(videos.Videos);
        Assert.Equal(
            new VideoResponse(video.Id, name, format, 2048, "PendingReview", null, Now.UtcDateTime),
            result.Value);
        Assert.Equal(bytes, storage.Files[video.FileName]);
        Assert.Equal(1, videos.SaveCount);
    }

    [Fact]
    public async Task Shortens_long_names()
    {
        var result = await Upload(new string('a', Video.NameMaxLength + 20) + ".mp4", VideoBytes.Mp4());

        Assert.Equal(Video.NameMaxLength, result.Value!.Name.Length);
    }

    [Fact]
    public async Task Accepts_exactly_10_mb()
    {
        var result = await Upload("big.mp4", VideoBytes.Mp4((int)Video.MaxSizeBytes));

        Assert.Equal(VideoStatus.Ok, result.Status);
        Assert.Equal(Video.MaxSizeBytes, result.Value!.SizeBytes);
    }

    [Theory]
    [InlineData("ad.mp4", "empty", "Choose a video file.")]
    [InlineData("ad.mp4", "large", "The video is larger than 10 MB.")]
    [InlineData("ad.mov", "mp4", "Only MP4 and WebM videos can be uploaded.")]
    [InlineData("ad", "mp4", "Only MP4 and WebM videos can be uploaded.")]
    [InlineData(null, "mp4", "Only MP4 and WebM videos can be uploaded.")]
    [InlineData("ad.mp4", "webm", "The file content is not an MP4 or WebM video.")]
    [InlineData("ad.webm", "mp4", "The file content is not an MP4 or WebM video.")]
    [InlineData("ad.mp4", "quicktime", "The file content is not an MP4 or WebM video.")]
    [InlineData("ad.webm", "matroska", "The file content is not an MP4 or WebM video.")]
    [InlineData("ad.mp4", "text", "The file content is not an MP4 or WebM video.")]
    public async Task Rejects_files_that_are_not_small_mp4_or_webm_videos(string? fileName, string content, string message)
    {
        var bytes = content switch
        {
            "empty" => [],
            "large" => VideoBytes.Mp4((int)Video.MaxSizeBytes + 1),
            "mp4" => VideoBytes.Mp4(),
            "webm" => VideoBytes.WebM(),
            "quicktime" => VideoBytes.QuickTime(),
            "matroska" => VideoBytes.Matroska(),
            _ => "<html>not a video</html>"u8.ToArray(),
        };

        var result = await Upload(fileName, bytes);

        Assert.Equal(VideoStatus.Invalid, result.Status);
        Assert.Equal([message], result.Errors["file"]);
        Assert.Empty(videos.Videos);
        Assert.Empty(storage.Files);
    }

    [Fact]
    public async Task Rejects_a_missing_file()
    {
        var result = await Service().UploadAsync(advertiser.Id, null, null);

        Assert.Equal(["Choose a video file."], result.Errors["file"]);
    }

    [Fact]
    public async Task Lists_the_company_videos_newest_first()
    {
        await Upload("first.mp4", VideoBytes.Mp4(), Now);
        await Upload("second.webm", VideoBytes.WebM(), Now.AddMinutes(5));
        var otherCompany = new Company("Other Co", null, Now.UtcDateTime);
        videos.Add(new Video(otherCompany.Id, "Not mine", VideoFormat.Mp4, 1, Now.UtcDateTime));

        var result = await Service().ListAsync(advertiser.Id);

        Assert.Equal(VideoStatus.Ok, result.Status);
        Assert.Equal(["second", "first"], result.Value!.Select(v => v.Name));
    }

    [Fact]
    public async Task Opens_a_company_video_for_preview()
    {
        var bytes = VideoBytes.WebM(512);
        var uploaded = (await Upload("ad.webm", bytes)).Value!;

        var result = await Service().OpenAsync(advertiser.Id, uploaded.Id);

        Assert.Equal(VideoStatus.Ok, result.Status);
        Assert.Equal("video/webm", result.Value!.ContentType);
        using var copy = new MemoryStream();
        await result.Value.Content.CopyToAsync(copy);
        Assert.Equal(bytes, copy.ToArray());
    }

    [Fact]
    public async Task Finds_no_unknown_foreign_or_missing_file_video()
    {
        var foreign = new Video(Guid.NewGuid(), "Not mine", VideoFormat.Mp4, 1, Now.UtcDateTime);
        var withoutFile = new Video(advertiser.CompanyId!.Value, "Lost", VideoFormat.Mp4, 1, Now.UtcDateTime);
        videos.Add(foreign);
        videos.Add(withoutFile);
        storage.Files[foreign.FileName] = [1];

        foreach (var id in new[] { Guid.NewGuid(), foreign.Id, withoutFile.Id })
        {
            Assert.Equal(VideoStatus.NotFound, (await Service().OpenAsync(advertiser.Id, id)).Status);
        }
    }

    [Fact]
    public async Task Only_known_advertisers_have_a_library()
    {
        var service = Service();
        var ghost = Guid.NewGuid();

        Assert.Equal(VideoStatus.UserNotFound, (await service.ListAsync(ghost)).Status);
        Assert.Equal(VideoStatus.UserNotFound, (await service.UploadAsync(ghost, "ad.mp4", new MemoryStream(VideoBytes.Mp4()))).Status);
        Assert.Equal(VideoStatus.UserNotFound, (await service.OpenAsync(ghost, Guid.NewGuid())).Status);
        Assert.Equal(VideoStatus.NotAdvertiser, (await service.ListAsync(admin.Id)).Status);
        Assert.Equal(VideoStatus.NotAdvertiser, (await service.UploadAsync(admin.Id, "ad.mp4", new MemoryStream(VideoBytes.Mp4()))).Status);
        Assert.Equal(VideoStatus.NotAdvertiser, (await service.OpenAsync(admin.Id, Guid.NewGuid())).Status);
        Assert.Empty(videos.Videos);
    }

    private VideoService Service(DateTimeOffset? now = null) =>
        new(users, videos, storage, new FixedTimeProvider(now ?? Now));

    private async Task<VideoResult<VideoResponse>> Upload(string? fileName, byte[] bytes, DateTimeOffset? now = null)
    {
        using var content = new MemoryStream(bytes);
        return await Service(now).UploadAsync(advertiser.Id, fileName, content);
    }
}
