using System.Net.Http.Headers;
using GestionCommerciale.Application.Interfaces.Security;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace GestionCommerciale.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static HttpClient CreateAuthenticatedClient(CustomWebApplicationFactory factory, UserRole role)
    {
        var email = role == UserRole.ADMIN
            ? CustomWebApplicationFactory.AdminEmail
            : CustomWebApplicationFactory.UserEmail;

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var user = db.Users.First(u => u.Email == email);
        var token = tokenGenerator.GenerateToken(user);

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
