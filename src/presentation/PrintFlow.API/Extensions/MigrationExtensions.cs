using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintFlow.Domain.Entities;
using PrintFlow.Persistence.Context;
using PrintFlow.Persistence.Seed;

namespace PrintFlow.API.Extensions
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
                return;

            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<PrintFlowDbContext>();
            await db.Database.MigrateAsync();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            await IdentitySeeder.SeedAdminAsync(userManager);
        }
    }
}
