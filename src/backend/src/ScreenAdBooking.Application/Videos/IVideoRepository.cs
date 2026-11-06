namespace ScreenAdBooking.Application.Videos;

public interface IVideoRepository
{
    /// <returns>Every video of the company, newest first.</returns>
    Task<IReadOnlyList<Video>> ListAsync(Guid companyId, CancellationToken cancellationToken);

    Task<Video?> FindAsync(Guid companyId, Guid videoId, CancellationToken cancellationToken);

    void Add(Video video);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

/// <summary>Where video files live. File names come from <see cref="Video.FileName"/>, never from user input.</summary>
public interface IVideoStorage
{
    Task SaveAsync(string fileName, Stream content, CancellationToken cancellationToken);

    /// <returns>A readable stream the caller disposes, or null when the file is missing.</returns>
    Stream? OpenRead(string fileName);
}
