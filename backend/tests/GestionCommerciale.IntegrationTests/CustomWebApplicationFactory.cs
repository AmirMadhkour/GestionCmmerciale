using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GestionCommerciale.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.com";
    public const string AdminPassword = "Admin123!";
    public const string UserEmail = "user@test.com";
    public const string UserPassword = "User123!";

    private readonly string _dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) || d.ServiceType == typeof(DbContextOptions))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_dbName));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedTestUsers(host.Services);
        return host;
    }

    private static void SeedTestUsers(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        db.Database.EnsureCreated();

        if (!db.Users.Any(u => u.Email == AdminEmail))
        {
            db.Users.Add(new User
            {
                Email = AdminEmail,
                PasswordHash = hasher.HashPassword(AdminPassword),
                FullName = "Admin Test",
                Role = UserRole.ADMIN
            });
        }

        if (!db.Users.Any(u => u.Email == UserEmail))
        {
            db.Users.Add(new User
            {
                Email = UserEmail,
                PasswordHash = hasher.HashPassword(UserPassword),
                FullName = "User Test",
                Role = UserRole.USER
            });
        }

        db.SaveChanges();
    }
}
