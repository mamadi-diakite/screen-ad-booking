namespace ScreenAdBooking.Infrastructure.Storage;

/// <summary>Keeps video files in one local folder, created on the first upload.</summary>
public sealed class FileSystemVideoStorage(string rootPath) : IVideoStorage
{
    public string RootPath { get; } = Path.GetFullPath(rootPath);

    public async Task SaveAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        Directory.CreateDirectory(RootPath);
        await using var file = new FileStream(PathOf(fileName), FileMode.CreateNew, FileAccess.Write, FileShare.None, 81_920, useAsync: true);
        await content.CopyToAsync(file, cancellationToken);
    }

    public Stream? OpenRead(string fileName)
    {
        var path = PathOf(fileName);

        return File.Exists(path)
            ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81_920, useAsync: true)
            : null;
    }

    private string PathOf(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        if (Path.GetFileName(fileName) != fileName)
        {
            throw new ArgumentException("Expected a bare file name.", nameof(fileName));
        }

        return Path.Combine(RootPath, fileName);
    }
}
