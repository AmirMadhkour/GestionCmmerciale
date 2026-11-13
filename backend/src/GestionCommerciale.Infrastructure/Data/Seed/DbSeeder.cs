using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context , IPasswordHasher passwordHasher)
    {
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                FullName = "AmirISI",
                Email = "Amir@isi.com.tn",
                PasswordHash = passwordHasher.HashPassword("Admin123!"),
                Role = UserRole.ADMIN
            };
            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}