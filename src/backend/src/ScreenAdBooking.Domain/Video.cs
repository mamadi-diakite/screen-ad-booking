namespace ScreenAdBooking.Domain;

public enum VideoFormat
{
    Mp4,
    WebM,
}

public enum VideoReviewStatus
{
    PendingReview,
    Approved,
    Rejected,
}

/// <summary>An advertiser's video. Only approved videos can be broadcast.</summary>
public sealed class Video
{
    public const int NameMaxLength = 200;

    public const int RejectionReasonMaxLength = 500;

    public const long MaxSizeBytes = 10 * 1024 * 1024;

    private Video()
    {
    }

    public Video(Guid companyId, string name, VideoFormat format, long sizeBytes, DateTime uploadedAtUtc)
    {
        Id = Guid.CreateVersion7();
        CompanyId = Guard.NotEmpty(companyId, nameof(companyId));
        Name = Guard.Required(name, nameof(name), NameMaxLength);
        Format = Guard.Defined(format, nameof(format));
        SizeBytes = Guard.Positive(sizeBytes, nameof(sizeBytes));
        if (sizeBytes > MaxSizeBytes)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "A video is 10 MB at most.");
        }

        UploadedAtUtc = Guard.Utc(uploadedAtUtc, nameof(uploadedAtUtc));
        Status = VideoReviewStatus.PendingReview;
    }

    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public VideoFormat Format { get; private set; }

    public long SizeBytes { get; private set; }

    public VideoReviewStatus Status { get; private set; }

    /// <summary>Set by the agency when it rejects the video.</summary>
    public string? RejectionReason { get; private set; }

    public DateTime UploadedAtUtc { get; private set; }

    /// <summary>Name of the stored file, derived from the id so user input never reaches the file system.</summary>
    public string FileName => $"{Id:N}.{(Format == VideoFormat.Mp4 ? "mp4" : "webm")}";

    public string ContentType => Format == VideoFormat.Mp4 ? "video/mp4" : "video/webm";
}
