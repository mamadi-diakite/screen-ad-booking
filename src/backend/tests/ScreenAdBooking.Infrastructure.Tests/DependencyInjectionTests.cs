namespace ScreenAdBooking.Infrastructure.Tests;

public sealed class DependencyInjectionTests
{
    private static readonly string VideosPath = Path.Combine(Path.GetTempPath(), "screen-ad-booking-tests", "videos");

    [Theory]
    [InlineData("", "videos")]
    [InlineData("  ", "videos")]
    [InlineData("Data Source=:memory:", "")]
    [InlineData("Data Source=:memory:", "  ")]
    public void Rejects_blank_connection_string_or_videos_path(string connectionString, string videosPath)
    {
        Assert.Throws<ArgumentException>(() => new ServiceCollection().AddInfrastructure(connectionString, videosPath));
    }

    [Fact]
    public void Registers_the_video_repository_and_file_storage()
    {
        using var provider = new ServiceCollection().AddInfrastructure("Data Source=:memory:", VideosPath).BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsType<VideoRepository>(scope.ServiceProvider.GetRequiredService<IVideoRepository>());
        var storage = Assert.IsType<FileSystemVideoStorage>(provider.GetRequiredService<IVideoStorage>());
        Assert.Equal(VideosPath, storage.RootPath);
    }

    [Fact]
    public async Task Creates_missing_data_folder_and_applies_migrations()
    {
        var root = Path.Combine(Path.GetTempPath(), "screen-ad-booking-tests", Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(root, "data", "app.db");

        try
        {
            await using var provider = new ServiceCollection()
                .AddInfrastructure($"Data Source={databasePath}", VideosPath)
                .BuildServiceProvider();

            await provider.ApplyMigrationsAsync();

            Assert.True(File.Exists(databasePath));
            await using var scope = provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<ScreenAdBookingDbContext>();
            Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Seeds_demo_data_once_through_the_service_provider()
    {
        await using var provider = new ServiceCollection()
            .AddInfrastructure("Data Source=seed-test;Mode=Memory;Cache=Shared", VideosPath)
            .BuildServiceProvider();

        // Keeps the shared in-memory database alive between scopes.
        await using var keepAlive = new SqliteConnection("Data Source=seed-test;Mode=Memory;Cache=Shared");
        await keepAlive.OpenAsync();

        await provider.ApplyMigrationsAsync();

        Assert.True(await provider.SeedDemoDataAsync());
        Assert.False(await provider.SeedDemoDataAsync());
    }

    [Theory]
    [InlineData("Data Source=:memory:")]
    [InlineData("Data Source=in-memory-test;Mode=Memory;Cache=Shared")]
    public async Task Applies_migrations_to_in_memory_database_without_touching_disk(string connectionString)
    {
        await using var provider = new ServiceCollection()
            .AddInfrastructure(connectionString, VideosPath)
            .BuildServiceProvider();

        await provider.ApplyMigrationsAsync();

        Assert.False(File.Exists("in-memory-test"));
    }
}
