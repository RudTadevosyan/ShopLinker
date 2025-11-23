namespace ShopLinkerService.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddEnvironmentVariables();

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHttpsRedirection();
        }


        //app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapControllers();
        
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            service = "ShopLinkerService",
            timestamp = DateTime.UtcNow
        }));

        app.Run();
    }
}