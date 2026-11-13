using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.UniTests.Domain;

public class ProductTests
{
    private static Product CreateProduct(int stock = 10) =>
        Product.Create(
            refe: "REF-001",
            name: "Clavier",
            desc: "Clavier USB",
            unitPriceHt: 100m,
            initialStock: stock);

    [Fact]
    public void DecreaseStock_WhenStockIsSufficient_DecreasesStockQuantity()
    {
        var product = CreateProduct(10);

        product.DecreaseStock(4);

        Assert.Equal(6, product.StockQuantity);
    }

    [Fact]
    public void DecreaseStock_WhenStockIsInsufficient_ThrowsBusinessRuleException()
    {
        var product = CreateProduct(3);

        Assert.Throws<BusinessRuleException>(() => product.DecreaseStock(4));
        Assert.Equal(3, product.StockQuantity);
    }

    [Fact]
    public void DecreaseStock_WhenQuantityIsNotPositive_ThrowsBusinessRuleException()
    {
        var product = CreateProduct();

        Assert.Throws<BusinessRuleException>(() => product.DecreaseStock(0));
    }

    [Fact]
    public void IncreaseStock_AddsToStockQuantity()
    {
        var product = CreateProduct(5);

        product.IncreaseStock(3);

        Assert.Equal(8, product.StockQuantity);
    }

    [Fact]
    public void Create_WhenPriceIsNegative_ThrowsBusinessRuleException()
    {
        Assert.Throws<BusinessRuleException>(() =>
            Product.Create("REF-002", "Souris", "Souris USB", -1m, 5));
    }
}
