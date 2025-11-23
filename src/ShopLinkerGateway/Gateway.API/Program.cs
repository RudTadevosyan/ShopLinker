using System.Text;
using Microsoft.IdentityModel.Tokens;
using Yarp.ReverseProxy.Transforms;

namespace Gateway.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        builder.Services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
            .AddTransforms(context =>
            {
                context.AddRequestTransform(transformContext =>
                {
                    var incomingHeaders = transformContext.HttpContext.Request.Headers;

                    if (incomingHeaders.TryGetValue("Authorization", out var authHeader))
                    {
                        // Use Add or TryAdd without indexing
                        transformContext.ProxyRequest.Headers.Remove("Authorization"); // optional, ensure no duplicates
                        transformContext.ProxyRequest.Headers.Add("Authorization", (string)authHeader!);
                    }

                    return ValueTask.CompletedTask;
                });
            });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllDev", policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            options.AddPolicy("ProductionCors", policy => policy
                .WithOrigins("https://yourdomain.com", "https://www.yourdomain.com") // Future prod domain (change)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
        
        // JWT
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);
        builder.Services.AddAuthentication(opt =>
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
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });
        
        
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors("AllowAllDev");
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
            app.UseHttpsRedirection();
            app.UseCors("ProductionCors");
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        
        app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Gateway", time = DateTime.UtcNow }));

        app.MapReverseProxy();

        app.Run();
    }
}
