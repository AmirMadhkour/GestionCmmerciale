using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Infrastructure.Data;
using GestionCommerciale.Infrastructure.Data.Seed;

namespace GestionCommerciale.Api.Extensions;

public static class DatabaseExtensions
{
    // Attention ici, on étend WebApplication (app) et non IServiceCollection (services)
    public static async Task ApplyMigrationsAndSeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            // On récupère le DbContext et le Hacheur de mot de passe depuis l'injection de dépendances
            var context = services.GetRequiredService<AppDbContext>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();

            // On appelle notre DbSeeder (créé dans le dossier Infrastructure)
            await DbSeeder.SeedAsync(context, passwordHasher);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Une erreur grave s'est produite lors de la migration ou de l'initialisation de la base de données.");
        }
    }
}