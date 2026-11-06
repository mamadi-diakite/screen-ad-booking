namespace ScreenAdBooking.Domain.Tests;

public sealed class VideoTests
{
    private static readonly Guid CompanyId = Guid.CreateVersion7();

    [Theory]
    [InlineData(VideoFormat.Mp4, "mp4", "video/mp4")]
    [InlineData(VideoFormat.WebM, "webm", "video/webm")]
    public void A_new_video_waits_for_review(VideoFormat format, string extension, string contentType)
    {
        var video = new Video(CompanyId, " Spring sale ", format, Video.MaxSizeBytes, TestDates.Now);

        Assert.NotEqual(Guid.Empty, video.Id);
        Assert.Equal(CompanyId, video.CompanyId);
        Assert.Equal("Spring sale", video.Name);
        Assert.Equal(format, video.Format);
        Assert.Equal(Video.MaxSizeBytes, video.SizeBytes);
        Assert.Equal(VideoReviewStatus.PendingReview, video.Status);
        Assert.Null(video.RejectionReason);
        Assert.Equal(TestDates.Now, video.UploadedAtUtc);
        Assert.Equal($"{video.Id:N}.{extension}", video.FileName);
        Assert.Equal(contentType, video.ContentType);
    }

    [Fact]
    public void Rejects_invalid_videos()
    {
        Assert.Equal("companyId", Assert.Throws<ArgumentException>(
            () => new Video(Guid.Empty, "Ad", VideoFormat.Mp4, 1, TestDates.Now)).ParamName);
        Assert.Equal("name", Assert.Throws<ArgumentException>(
            () => new Video(CompanyId, " ", VideoFormat.Mp4, 1, TestDates.Now)).ParamName);
        Assert.Equal("format", Assert.Throws<ArgumentOutOfRangeException>(
            () => new Video(CompanyId, "Ad", (VideoFormat)9, 1, TestDates.Now)).ParamName);
        Assert.Equal("sizeBytes", Assert.Throws<ArgumentOutOfRangeException>(
            () => new Video(CompanyId, "Ad", VideoFormat.Mp4, 0, TestDates.Now)).ParamName);
        Assert.Equal("sizeBytes", Assert.Throws<ArgumentOutOfRangeException>(
            () => new Video(CompanyId, "Ad", VideoFormat.Mp4, Video.MaxSizeBytes + 1, TestDates.Now)).ParamName);
        Assert.Equal("uploadedAtUtc", Assert.Throws<ArgumentException>(
            () => new Video(CompanyId, "Ad", VideoFormat.Mp4, 1, DateTime.SpecifyKind(TestDates.Now, DateTimeKind.Local))).ParamName);
    }
}
