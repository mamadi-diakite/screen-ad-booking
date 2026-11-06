namespace ScreenAdBooking.Api.IntegrationTests;

public sealed class DatabaseStartupTests(ScreenAdBookingApiFactory factory) : IClassFixture<ScreenAdBookingApiFactory>
{
    [Fact]
    public async Task Startup_applies_all_migrations()
    {
        _ = factory.CreateClient();

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ScreenAdBookingDbContext>();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(0, await db.Users.CountAsync());
    }
}
