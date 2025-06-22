using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace TrashBoard.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var roles = new[] { "Admin", "Viewer" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = "admin@trashboard.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        var viewerEmail = "viewer@trashboard.local";
        var viewerUser = await userManager.FindByEmailAsync(viewerEmail);
        if (viewerUser == null)
        {
            viewerUser = new IdentityUser { UserName = viewerEmail, Email = viewerEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(viewerUser, "Viewer123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(viewerUser, "Viewer");
        }
    }
}
