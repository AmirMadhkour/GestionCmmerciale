
namespace GestionCommerciale.Api.Extensions;

public static class AuthorizationPolicies
{
    public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
            
            
            options.AddPolicy("AllUsers", policy => policy.RequireRole("ADMIN", "USER"));
        });

        return services;
    }
}