using System.Net;
using System.Net.Http.Json;
using GestionCommerciale.Application.DTOs.Auth;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.IntegrationTests.Helpers;

namespace GestionCommerciale.IntegrationTests.Controllers;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndRole()
    {
        var client = _factory.CreateClient();
        var request = new LoginDto
        {
            Email = CustomWebApplicationFactory.AdminEmail,
            Password = CustomWebApplicationFactory.AdminPassword
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal("ADMIN", body.Role);
        Assert.Equal(CustomWebApplicationFactory.AdminEmail, body.Email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();
        var request = new LoginDto { Email = CustomWebApplicationFactory.AdminEmail, Password = "WrongPassword1" };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithEmptyFields_Returns400BadRequest()
    {
        var client = _factory.CreateClient();
        var request = new LoginDto { Email = string.Empty, Password = string.Empty };

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutToken_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUserClaims()
    {
        var client = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.USER);

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(CustomWebApplicationFactory.UserEmail, content);
        Assert.Contains("USER", content);
    }
}
