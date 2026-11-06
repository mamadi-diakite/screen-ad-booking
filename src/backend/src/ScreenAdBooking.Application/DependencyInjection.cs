using Microsoft.Extensions.DependencyInjection;
using ScreenAdBooking.Application.Auth;

namespace ScreenAdBooking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();

        return services;
    }
}
