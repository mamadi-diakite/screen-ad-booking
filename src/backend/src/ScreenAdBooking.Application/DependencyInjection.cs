namespace ScreenAdBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<VenueSearchService>();
        services.AddScoped<CreditService>();
        services.AddScoped<VideoService>();

        return services;
    }
}
