namespace ScreenAdBooking.Application.Videos;

public enum VideoStatus
{
    Ok,
    UserNotFound,
    NotAdvertiser,
    NotFound,
    Invalid,
}

/// <param name="Errors">Messages by field name in camelCase when <paramref name="Status"/> is Invalid.</param>
public sealed record VideoResult<T>(VideoStatus Status, T? Value, IReadOnlyDictionary<string, string[]> Errors)
    where T : class
{
    public static VideoResult<T> Of(VideoStatus status) => new(status, null, new Dictionary<string, string[]>());

    public static VideoResult<T> Ok(T value) => new(VideoStatus.Ok, value, new Dictionary<string, string[]>());
}

/// <param name="Content">Stream the caller disposes.</param>
public sealed record VideoContent(Stream Content, string ContentType);

public sealed class VideoService(
    IUserRepository users,
    IVideoRepository videos,
    IVideoStorage storage,
    TimeProvider timeProvider)
{
    public const string UntitledName = "Untitled video";

    private const string NotAVideoMessage = "The file content is not an MP4 or WebM video.";

    public async Task<VideoResult<IReadOnlyList<VideoResponse>>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (status, companyId) = await FindCompanyAsync(userId, cancellationToken);
        if (status != VideoStatus.Ok)
        {
            return VideoResult<IReadOnlyList<VideoResponse>>.Of(status);
        }

        var list = await videos.ListAsync(companyId, cancellationToken);

        return VideoResult<IReadOnlyList<VideoResponse>>.Ok([.. list.Select(ToResponse)]);
    }

    /// <summary>Stores an MP4 or WebM video of 10 MB at most. The video waits for the agency's review.</summary>
    /// <param name="fileName">Name of the uploaded file; gives the expected format and the video name.</param>
    /// <param name="content">Uploaded content; null when no file was sent.</param>
    public async Task<VideoResult<VideoResponse>> UploadAsync(
        Guid userId,
        string? fileName,
        Stream? content,
        CancellationToken cancellationToken = default)
    {
        var (status, companyId) = await FindCompanyAsync(userId, cancellationToken);
        if (status != VideoStatus.Ok)
        {
            return VideoResult<VideoResponse>.Of(status);
        }

        if (content is null)
        {
            return Invalid("Choose a video file.");
        }

        VideoFormat? expected = (Path.GetExtension(fileName) ?? string.Empty).ToUpperInvariant() switch
        {
            ".MP4" => VideoFormat.Mp4,
            ".WEBM" => VideoFormat.WebM,
            _ => null,
        };
        if (expected is null)
        {
            return Invalid("Only MP4 and WebM videos can be uploaded.");
        }

        using var buffer = new MemoryStream();
        await CopyAtMostAsync(content, buffer, Video.MaxSizeBytes + 1, cancellationToken);
        if (buffer.Length == 0)
        {
            return Invalid("Choose a video file.");
        }

        if (buffer.Length > Video.MaxSizeBytes)
        {
            return Invalid("The video is larger than 10 MB.");
        }

        if (VideoSignature.Detect(buffer.GetBuffer().AsSpan(0, (int)buffer.Length)) != expected)
        {
            return Invalid(NotAVideoMessage);
        }

        var video = new Video(companyId, NameOf(fileName!), expected.Value, buffer.Length, timeProvider.GetUtcNow().UtcDateTime);
        buffer.Position = 0;
        await storage.SaveAsync(video.FileName, buffer, cancellationToken);
        videos.Add(video);
        await videos.SaveChangesAsync(cancellationToken);

        return VideoResult<VideoResponse>.Ok(ToResponse(video));
    }

    /// <summary>Opens one of the company's videos for preview.</summary>
    public async Task<VideoResult<VideoContent>> OpenAsync(Guid userId, Guid videoId, CancellationToken cancellationToken = default)
    {
        var (status, companyId) = await FindCompanyAsync(userId, cancellationToken);
        if (status != VideoStatus.Ok)
        {
            return VideoResult<VideoContent>.Of(status);
        }

        var video = await videos.FindAsync(companyId, videoId, cancellationToken);
        var content = video is null ? null : storage.OpenRead(video.FileName);

        return content is null
            ? VideoResult<VideoContent>.Of(VideoStatus.NotFound)
            : VideoResult<VideoContent>.Ok(new VideoContent(content, video!.ContentType));
    }

    private static VideoResult<VideoResponse> Invalid(string message) =>
        new(VideoStatus.Invalid, null, new Dictionary<string, string[]> { ["file"] = [message] });

    private static string NameOf(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName).Trim();
        if (name.Length == 0)
        {
            return UntitledName;
        }

        return name.Length > Video.NameMaxLength ? name[..Video.NameMaxLength].TrimEnd() : name;
    }

    private static async Task CopyAtMostAsync(Stream source, Stream target, long limit, CancellationToken cancellationToken)
    {
        var chunk = new byte[81_920];
        int read;
        while ((read = await source.ReadAsync(chunk.AsMemory(0, (int)Math.Min(chunk.Length, limit - target.Length)), cancellationToken)) > 0)
        {
            await target.WriteAsync(chunk.AsMemory(0, read), cancellationToken);
        }
    }

    private static VideoResponse ToResponse(Video video) => new(
        video.Id,
        video.Name,
        video.Format.ToString(),
        video.SizeBytes,
        video.Status.ToString(),
        video.RejectionReason,
        video.UploadedAtUtc);

    private async Task<(VideoStatus Status, Guid CompanyId)> FindCompanyAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);

        return user switch
        {
            null => (VideoStatus.UserNotFound, Guid.Empty),
            { CompanyId: { } companyId } => (VideoStatus.Ok, companyId),
            _ => (VideoStatus.NotAdvertiser, Guid.Empty),
        };
    }
}
