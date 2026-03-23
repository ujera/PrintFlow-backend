using Microsoft.AspNetCore.Identity;
using PrintFlow.Domain.Entities;
using PrintFlow.Domain.Enums;

namespace PrintFlow.Persistence.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAdminAsync(UserManager<User> userManager)
    {
        const string adminEmail = "admin@printflow.io";

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is not null)
            return;

        var admin = new User
        {
            Id = Guid.Parse("a1b2c3d4-0000-0000-0000-000000000001"),
            UserName = adminEmail,
            Email = adminEmail,
            Name = "PrintFlow Admin",
            Role = UserRole.Admin,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}