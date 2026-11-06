namespace ScreenAdBooking.Application.Tests;

public sealed class ApplicationDependencyInjectionTests
{
    [Fact]
    public void Registers_the_application_services()
    {
        var services = new ServiceCollection()
            .AddApplication()
            .AddSingleton(TimeProvider.System)
            .AddSingleton<IUserRepository, InMemoryUserRepository>()
            .AddSingleton<IPasswordHasher, FakePasswordHasher>()
            .AddSingleton<IAccessTokenIssuer, FakeTokenIssuer>()
            .AddSingleton<INetworkQueries, FakeNetworkQueries>()
            .AddSingleton<ICreditRepository, InMemoryCreditRepository>()
            .AddSingleton<IVideoRepository, InMemoryVideoRepository>()
            .AddSingleton<IVideoStorage, InMemoryVideoStorage>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<AuthService>());
        Assert.NotNull(scope.ServiceProvider.GetService<VenueSearchService>());
        Assert.NotNull(scope.ServiceProvider.GetService<CreditService>());
        Assert.NotNull(scope.ServiceProvider.GetService<VideoService>());
    }
}
