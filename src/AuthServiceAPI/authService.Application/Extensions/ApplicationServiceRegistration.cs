using authService.Application.Helpers.ProfileMappers;
using authService.Application.Services;
using authService.Domain.Interfaces;
using authService.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace authService.Application.Extensions;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // AutoMapper & Application services DI
        services.AddAutoMapper(_ => { }, typeof(UserProfile));
        services.AddScoped<IAuthJwtService, AuthJwtService>();

        // Call Infrastructure registration internally
        services.AddInfrastructureServices(configuration);

        return services;
    }
    
}