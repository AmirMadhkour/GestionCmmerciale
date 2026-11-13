using System.Net;
using System.Net.Http.Json;
using GestionCommerciale.Application.DTOs.Clients;
using GestionCommerciale.Application.DTOs.Orders;
using GestionCommerciale.Application.DTOs.Products;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.IntegrationTests.Helpers;

namespace GestionCommerciale.IntegrationTests.Controllers;

public class OrdersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OrdersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task<ClientDto> CreateClientAsync(HttpClient admin, string email)
    {
        var dto = new CreateClientDto
        {
            LastName = "Madhkour",
            FirstNameOrCompany = "Amir",
            Email = email,
            Phone = "56103009",
            Address = new[] { "Tunis" }
        };
        var response = await admin.PostAsJsonAsync("/api/clients", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ClientDto>())!;
    }

    private static async Task<ProductDto> CreateProductAsync(HttpClient admin, string reference, int stock, decimal price = 100m)
    {
        var dto = new CreateProductDto
        {
            Reference = reference,
            Name = "PC Portable",
            Description = "i7",
            UnitPriceHT = price,
            InitialStock = stock
        };
        var response = await admin.PostAsJsonAsync("/api/products", dto);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductDto>())!;
    }

    private static CreateOrderDto OrderFor(Guid clientId, Guid productId, int quantity) => new()
    {
        ClientId = clientId,
        Lines = new List<OrderLineInputDto> { new() { ProductID = productId, Quantity = quantity } }
    };

    private static async Task<int> StockOfAsync(HttpClient client, Guid productId)
    {
        var product = await client.GetFromJsonAsync<ProductDto>($"/api/products/{productId}");
        return product!.StockQuantity;
    }

    [Fact]
    public async Task FullOrderLifecycle_CreateValidateAndCheckStockDecrement_WorksCorrectly()
    {
        var admin = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);
        var user = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.USER);
        var client = await CreateClientAsync(admin, "amir.order@test.com");
        var product = await CreateProductAsync(admin, "REF-ORD-1", 10);

        var orderResponse = await user.PostAsJsonAsync("/api/orders", OrderFor(client.Id, product.Id, 3));

        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailDto>();
        Assert.NotNull(order);
        Assert.Equal(300m, order.TotalHt);
        Assert.Equal(357m, order.TotalTTC);
        Assert.Equal(10, await StockOfAsync(admin, product.Id));

        var forbidden = await user.PostAsync($"/api/orders/{order.Id}/validate", null);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var validated = await admin.PostAsync($"/api/orders/{order.Id}/validate", null);
        validated.EnsureSuccessStatusCode();
        Assert.Equal(7, await StockOfAsync(admin, product.Id));
    }

    [Fact]
    public async Task CreateOrder_WhenQuantityExceedsStock_Returns400AndKeepsStock()
    {
        var admin = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);
        var client = await CreateClientAsync(admin, "amir.stock@test.com");
        var product = await CreateProductAsync(admin, "REF-ORD-2", 5);

        var response = await admin.PostAsJsonAsync("/api/orders", OrderFor(client.Id, product.Id, 6));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(5, await StockOfAsync(admin, product.Id));
    }

    [Fact]
    public async Task CreateOrder_WhenQuantityIsZero_Returns400BadRequest()
    {
        var admin = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);
        var client = await CreateClientAsync(admin, "amir.zero@test.com");
        var product = await CreateProductAsync(admin, "REF-ORD-3", 5);

        var response = await admin.PostAsJsonAsync("/api/orders", OrderFor(client.Id, product.Id, 0));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CancelValidatedOrder_RestoresStock_ButCancellingADraftDoesNot()
    {
        var admin = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);
        var client = await CreateClientAsync(admin, "amir.cancel@test.com");
        var product = await CreateProductAsync(admin, "REF-ORD-4", 10);

        var validatedOrder = await (await admin.PostAsJsonAsync("/api/orders", OrderFor(client.Id, product.Id, 4)))
            .Content.ReadFromJsonAsync<OrderDetailDto>();
        (await admin.PostAsync($"/api/orders/{validatedOrder!.Id}/validate", null)).EnsureSuccessStatusCode();
        Assert.Equal(6, await StockOfAsync(admin, product.Id));

        (await admin.PostAsync($"/api/orders/{validatedOrder.Id}/cancel", null)).EnsureSuccessStatusCode();
        Assert.Equal(10, await StockOfAsync(admin, product.Id));

        var draftOrder = await (await admin.PostAsJsonAsync("/api/orders", OrderFor(client.Id, product.Id, 2)))
            .Content.ReadFromJsonAsync<OrderDetailDto>();
        (await admin.PostAsync($"/api/orders/{draftOrder!.Id}/cancel", null)).EnsureSuccessStatusCode();
        Assert.Equal(10, await StockOfAsync(admin, product.Id));
    }

    [Fact]
    public async Task DeleteClient_WhenClientHasOrders_Returns409Conflict()
    {
        var admin = AuthHelper.CreateAuthenticatedClient(_factory, UserRole.ADMIN);
        var client = await CreateClientAsync(admin, "amir.delete@test.com");
        var product = await CreateProductAsync(admin, "REF-ORD-5", 5);
        (await admin.PostAsJsonAsync("/api/orders", OrderFor(client.Id, product.Id, 1))).EnsureSuccessStatusCode();

        var response = await admin.DeleteAsync($"/api/clients/{client.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
