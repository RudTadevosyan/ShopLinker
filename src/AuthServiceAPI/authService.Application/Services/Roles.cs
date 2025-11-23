using Microsoft.AspNetCore.Identity;

namespace authService.Application.Services;

public static class Roles
{
    // creating roles
    public static async Task AddRoles(RoleManager<IdentityRole<int>> roleManager)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));

        if (!await roleManager.RoleExistsAsync("Client"))
            await roleManager.CreateAsync(new IdentityRole<int>("Client"));
    }
}