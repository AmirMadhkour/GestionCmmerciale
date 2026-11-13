using System.Net;
using System.Net.Http.Json;
using GestionCommerciale.Application.DTOs.Products;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.IntegrationTests.Helpers;

namespace GestionCommerciale.IntegrationTests.Controllers;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static CreateProductDto CreateDto(string reference, string name = "Ecran") => new()
    {
        Reference = reference,
        Name = name,
        Description = "Description",
        UnitPriceHT = 300m,
        InitialStock = 10
    };

    [Fact]
    public async Task CreateProduct_WhenUnauthenticated_Returns401Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", CreateDto("REF-401"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WhenRoleIsUser_Returns403Forbidden()
    {
        var userClient = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.USER);

        var response = await userClient.PostAsJsonAsync("/api/products", CreateDto("REF-403"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WhenRoleIsAdmin_Returns201Created()
    {
        var adminClient = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);

        var response = await adminClient.PostAsJsonAsync("/api/products", CreateDto("REF-201", "Clavier"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(created);
        Assert.Equal("REF-201", created.Reference);
        Assert.Equal(10, created.StockQuantity);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateReference_Returns409Conflict()
    {
        var adminClient = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);
        await adminClient.PostAsJsonAsync("/api/products", CreateDto("REF-DUP"));

        var response = await adminClient.PostAsJsonAsync("/api/products", CreateDto("REF-DUP"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithEmptyName_Returns400BadRequest()
    {
        var adminClient = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);

        var response = await adminClient.PostAsJsonAsync("/api/products", CreateDto("REF-BAD", string.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_WithUnknownId_Returns404NotFound()
    {
        var userClient = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.USER);

        var response = await userClient.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
