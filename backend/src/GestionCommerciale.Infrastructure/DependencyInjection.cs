using GestionCommerciale.Application.Common.Interfaces;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Infrastructure.Data;
using GestionCommerciale.Infrastructure.Repositories;
using GestionCommerciale.Infrastructure.Security;
using GestionCommerciale.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}