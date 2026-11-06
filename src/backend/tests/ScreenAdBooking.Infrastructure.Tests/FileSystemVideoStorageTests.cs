namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class FileSystemVideoStorageTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "screen-ad-booking-tests", Guid.NewGuid().ToString("N"), "videos");

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task Creates_the_folder_then_saves_and_reads_back_a_file()
    {
        var storage = new FileSystemVideoStorage(_root);
        byte[] bytes = [1, 2, 3, 4];

        await storage.SaveAsync("clip.mp4", new MemoryStream(bytes), default);

        Assert.True(File.Exists(Path.Combine(_root, "clip.mp4")));
        await using var read = storage.OpenRead("clip.mp4")!;
        using var copy = new MemoryStream();
        await read.CopyToAsync(copy);
        Assert.Equal(bytes, copy.ToArray());
    }

    [Fact]
    public async Task Never_overwrites_a_file()
    {
        var storage = new FileSystemVideoStorage(_root);
        await storage.SaveAsync("clip.mp4", new MemoryStream([1]), default);

        await Assert.ThrowsAsync<IOException>(() => storage.SaveAsync("clip.mp4", new MemoryStream([2]), default));
    }

    [Fact]
    public void Returns_null_for_a_missing_file()
    {
        Assert.Null(new FileSystemVideoStorage(_root).OpenRead("missing.mp4"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../escape.mp4")]
    [InlineData("nested/clip.mp4")]
    public async Task Accepts_bare_file_names_only(string fileName)
    {
        var storage = new FileSystemVideoStorage(_root);

        Assert.Throws<ArgumentException>(() => storage.OpenRead(fileName));
        await Assert.ThrowsAsync<ArgumentException>(() => storage.SaveAsync(fileName, new MemoryStream([1]), default));
    }

    [Fact]
    public async Task Rejects_a_missing_stream()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => new FileSystemVideoStorage(_root).SaveAsync("clip.mp4", null!, default));
    }
}
