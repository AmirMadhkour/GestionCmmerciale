using FluentValidation;
using GestionCommerciale.Application.Interfaces.Services;
using GestionCommerciale.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace GestionCommerciale.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();

  
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}