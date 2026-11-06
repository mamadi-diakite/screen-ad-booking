using Microsoft.Extensions.DependencyInjection;
using ScreenAdBooking.Application.Auth;
using ScreenAdBooking.Application.Credits;
using ScreenAdBooking.Application.Network;

namespace ScreenAdBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<VenueSearchService>();
        services.AddScoped<CreditService>();

        return services;
    }
}
