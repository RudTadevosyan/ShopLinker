using System.Text;
using authService.Domain.Entities;
using authService.Domain.Interfaces;
using authService.Infrastructure.DataBases;
using authService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace authService.Infrastructure.Extensions;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Database 
        var conn = configuration.GetConnectionString("AuthServiceDbContext");
        services.AddDbContext<AuthServiceDbContext>
            (opt => opt.UseNpgsql(conn, x => x.MigrationsAssembly("authService.Infrastructure")));
        
        services.AddIdentity<User, IdentityRole<int>>()
            .AddEntityFrameworkStores<AuthServiceDbContext>()
            .AddDefaultTokenProviders();

        // JWT
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!);
        services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        
        
        // Repositories DI
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        return services;
    }
}