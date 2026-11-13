namespace GestionCommerciale.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCustomCors(this IServiceCollection services, string policyName)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(policyName, builder =>
            {
                builder.WithOrigins("http://localhost:4200") // URL de ton Angular (à mettre dans appsettings plus tard)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials(); // Requis pour envoyer le token d'autorisation
            });
        });

        return services;
    }
}