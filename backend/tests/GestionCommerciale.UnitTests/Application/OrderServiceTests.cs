using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Orders;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Services;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Domain.Exceptions;
using Moq;

namespace GestionCommerciale.UniTests.Application;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IClientRepository> _clientRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _clientRepoMock = new Mock<IClientRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new OrderService(
            _orderRepoMock.Object,
            _unitOfWorkMock.Object,
            _clientRepoMock.Object,
            _productRepoMock.Object);
    }

    private static Client CreateClient() =>
        Client.Create("Madhkour", "Amir", "amir@test.com", "56103009", new[] { "Tunis" });

    private static Product CreateProduct(int stock = 10, decimal price = 100m) =>
        Product.Create("REF-001", "Clavier", "Clavier USB", price, stock);

    private static CreateOrderDto CreateDto(Guid clientId, Guid productId, int quantity) => new()
    {
        ClientId = clientId,
        Lines = new List<OrderLineInputDto> { new() { ProductID = productId, Quantity = quantity } }
    };

    private void ArrangeCreateContext(Client client, Product product)
    {
        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _orderRepoMock.Setup(r => r.GenerateOrderNumberAsync()).ReturnsAsync("CMD-202611-0001");
    }

    private void ArrangeExistingOrder(Order order, Product product)
    {
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(order.Id)).ReturnsAsync(order);
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestedQuantityExceedsStock_ThrowsBusinessRuleException()
    {
        var client = CreateClient();
        var product = CreateProduct(stock: 3);
        ArrangeCreateContext(client, product);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(CreateDto(client.Id, product.Id, 5)));

        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenQuantityIsNotPositive_ThrowsBusinessRuleException()
    {
        var client = CreateClient();
        var product = CreateProduct();
        ArrangeCreateContext(client, product);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CreateAsync(CreateDto(client.Id, product.Id, 0)));
    }

    [Fact]
    public async Task CreateAsync_WhenClientDoesNotExist_ThrowsNotFoundException()
    {
        var product = CreateProduct();
        var clientId = Guid.NewGuid();
        _clientRepoMock.Setup(r => r.GetByIdAsync(clientId)).ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(CreateDto(clientId, product.Id, 1)));
    }

    [Fact]
    public async Task CreateAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        var client = CreateClient();
        var productId = Guid.NewGuid();
        _clientRepoMock.Setup(r => r.GetByIdAsync(client.Id)).ReturnsAsync(client);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);
        _orderRepoMock.Setup(r => r.GenerateOrderNumberAsync()).ReturnsAsync("CMD-202611-0001");

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateAsync(CreateDto(client.Id, productId, 1)));
    }

    [Fact]
    public async Task CreateAsync_WhenDataIsValid_CreatesDraftWith19PercentVatAndKeepsStockUntouched()
    {
        var client = CreateClient();
        var product = CreateProduct(stock: 10, price: 100m);
        ArrangeCreateContext(client, product);

        var result = await _sut.CreateAsync(CreateDto(client.Id, product.Id, 2));

        Assert.Equal("CMD-202611-0001", result.OrderNumber);
        Assert.Equal(OrderStatus.DRAFT.ToString(), result.OrderStatus);
        Assert.Equal(200m, result.TotalHt);
        Assert.Equal(238m, result.TotalTTC);
        Assert.Equal(client.Id, result.ClientId);
        Assert.Equal("Madhkour Amir", result.ClientName);
        Assert.Equal(10, product.StockQuantity);
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesTheLinesOfADraftOrder()
    {
        var product = CreateProduct(stock: 10);
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(product, 2);
        ArrangeExistingOrder(order, product);

        var dto = new UpdateOrderDto
        {
            Lines = new List<OrderLineInputDto> { new() { ProductID = product.Id, Quantity = 4 } }
        };

        await _sut.UpdateAsync(order.Id, dto);

        Assert.Equal(4, Assert.Single(order.Lines).Quantity);
        Assert.Equal(400m, order.TotalHt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_WhenStockIsSufficient_DecreasesProductStockAndValidatesOrder()
    {
        var product = CreateProduct(stock: 10);
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(product, 4);
        ArrangeExistingOrder(order, product);

        await _sut.ValidateAsync(order.Id);

        Assert.Equal(6, product.StockQuantity);
        Assert.Equal(OrderStatus.VALIDATED, order.OrderStatus);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateAsync_WhenStockBecomesInsufficient_ThrowsAndSavesNothing()
    {
        var product = CreateProduct(stock: 5);
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(product, 5);
        product.DecreaseStock(4);
        ArrangeExistingOrder(order, product);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.ValidateAsync(order.Id));

        Assert.Equal(1, product.StockQuantity);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidateAsync_WhenOrderDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetOrderWithDetailsAsync(id)).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ValidateAsync(id));
    }

    [Fact]
    public async Task CancelAsync_WhenOrderWasValidated_RestoresProductStock()
    {
        var product = CreateProduct(stock: 10);
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(product, 4);
        ArrangeExistingOrder(order, product);
        await _sut.ValidateAsync(order.Id);
        Assert.Equal(6, product.StockQuantity);

        await _sut.CancelAsync(order.Id);

        Assert.Equal(10, product.StockQuantity);
        Assert.Equal(OrderStatus.CANCELLED, order.OrderStatus);
    }

    [Fact]
    public async Task CancelAsync_WhenOrderIsStillDraft_DoesNotChangeProductStock()
    {
        var product = CreateProduct(stock: 10);
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(product, 4);
        ArrangeExistingOrder(order, product);

        await _sut.CancelAsync(order.Id);

        Assert.Equal(10, product.StockQuantity);
        Assert.Equal(OrderStatus.CANCELLED, order.OrderStatus);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_WhenOrderIsAlreadyCancelled_ThrowsBusinessRuleException()
    {
        var product = CreateProduct();
        var order = Order.Create(Guid.NewGuid());
        order.AddLine(product, 1);
        order.Cancel();
        ArrangeExistingOrder(order, product);

        await Assert.ThrowsAsync<BusinessRuleException>(() => _sut.CancelAsync(order.Id));
    }
}
