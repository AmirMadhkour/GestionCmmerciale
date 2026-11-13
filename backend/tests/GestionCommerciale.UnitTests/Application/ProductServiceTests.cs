using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Products;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Services;
using GestionCommerciale.Domain.Entities;
using Moq;

namespace GestionCommerciale.UniTests.Application;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ProductService(_productRepoMock.Object, _unitOfWorkMock.Object);
    }

    private static Product CreateSampleProduct(string reference = "REF-001", int stock = 10) =>
        Product.Create(reference, "Clavier", "Clavier USB", 100m, stock);

    private static CreateProductDto CreateDto(string reference = "REF-001") => new()
    {
        Reference = reference,
        Name = "Clavier",
        Description = "Clavier USB",
        UnitPriceHT = 100m,
        InitialStock = 10
    };

    [Fact]
    public async Task CreateAsync_WhenReferenceIsUnique_CreatesAndReturnsProduct()
    {
        _productRepoMock
            .Setup(r => r.ReferenceExistsAsync("REF-001"))
            .ReturnsAsync(false);

        var result = await _sut.CreateAsync(CreateDto());

        Assert.Equal("REF-001", result.Reference);
        Assert.Equal(100m, result.UnitPriceHT);
        Assert.Equal(10, result.StockQuantity);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenReferenceAlreadyExists_ThrowsConflictException()
    {
        _productRepoMock
            .Setup(r => r.ReferenceExistsAsync("REF-001"))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateAsync(CreateDto()));

        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        var product = CreateSampleProduct();
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(product.Id);

        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Clavier", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewReferenceBelongsToAnotherProduct_ThrowsConflictException()
    {
        var product = CreateSampleProduct("REF-001");
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.ReferenceExistsAsync("REF-002")).ReturnsAsync(true);

        var dto = new UpdateProductDto { Reference = "REF-002", Name = "Clavier", Description = "d", UnitPriceHT = 90m };

        await Assert.ThrowsAsync<ConflictException>(() => _sut.UpdateAsync(product.Id, dto));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductIsUsedInOrders_ThrowsConflictException()
    {
        var product = CreateSampleProduct();
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.IsUsedInOrdersAsync(product.Id)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.DeleteAsync(product.Id));

        _productRepoMock.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductIsNotUsed_RemovesAndSaves()
    {
        var product = CreateSampleProduct();
        _productRepoMock.Setup(r => r.GetByIdAsync(product.Id)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.IsUsedInOrdersAsync(product.Id)).ReturnsAsync(false);

        await _sut.DeleteAsync(product.Id);

        _productRepoMock.Verify(r => r.Remove(product), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
