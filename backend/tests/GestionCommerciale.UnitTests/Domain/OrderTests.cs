using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.UniTests.Domain;

public class OrderTests
{
    private static Order CreateDraftOrder() =>
        Order.Create(Guid.NewGuid());

    private static Product CreateProduct(decimal priceHt = 100m, int stock = 10, string reference = "REF-001") =>
        Product.Create(reference, "Produit " + reference, "Description", priceHt, stock);

    [Fact]
    public void Create_WithEmptyClientId_ThrowsBusinessRuleException()
    {
        Assert.Throws<BusinessRuleException>(() => Order.Create(Guid.Empty));
    }

    [Fact]
    public void Create_StartsAsDraftWithZeroTotals()
    {
        var order = CreateDraftOrder();

        Assert.Equal(OrderStatus.DRAFT, order.OrderStatus);
        Assert.Empty(order.Lines);
        Assert.Equal(0m, order.TotalHt);
        Assert.Equal(0m, order.TotalTtc);
    }

    [Fact]
    public void AddLine_WithValidData_CalculatesLineTotalAndOrderTotalsWith19PercentVat()
    {
        var order = CreateDraftOrder();
        var product1 = CreateProduct(100m, 10, "REF-001");
        var product2 = CreateProduct(50m, 10, "REF-002");

        order.AddLine(product1, Q: 2);
        order.AddLine(product2, Q: 3);

        Assert.Equal(2, order.Lines.Count);
        Assert.Equal(200m, order.Lines.First(l => l.ProductId == product1.Id).LineTotal);
        Assert.Equal(150m, order.Lines.First(l => l.ProductId == product2.Id).LineTotal);
        Assert.Equal(350m, order.TotalHt);
        Assert.Equal(416.5m, order.TotalTtc);
    }

    [Fact]
    public void AddLine_WithSameProductTwice_MergesQuantitiesIntoOneLine()
    {
        var order = CreateDraftOrder();
        var product = CreateProduct(100m, 10);

        order.AddLine(product, Q: 2);
        order.AddLine(product, Q: 3);

        var line = Assert.Single(order.Lines);
        Assert.Equal(5, line.Quantity);
        Assert.Equal(500m, order.TotalHt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddLine_WithNonPositiveQuantity_ThrowsBusinessRuleException(int quantity)
    {
        var order = CreateDraftOrder();
        var product = CreateProduct();

        Assert.Throws<BusinessRuleException>(() => order.AddLine(product, quantity));
        Assert.Empty(order.Lines);
    }

    [Fact]
    public void AddLine_WithQuantityAboveStock_ThrowsBusinessRuleException()
    {
        var order = CreateDraftOrder();
        var product = CreateProduct(stock: 5);

        Assert.Throws<BusinessRuleException>(() => order.AddLine(product, 6));
        Assert.Empty(order.Lines);
    }

    [Fact]
    public void AddLine_WhenCumulatedQuantityExceedsStock_ThrowsAndKeepsPreviousLine()
    {
        var order = CreateDraftOrder();
        var product = CreateProduct(stock: 5);
        order.AddLine(product, 3);

        Assert.Throws<BusinessRuleException>(() => order.AddLine(product, 3));

        Assert.Equal(3, Assert.Single(order.Lines).Quantity);
    }

    [Fact]
    public void AddLine_DoesNotChangeProductStock()
    {
        var order = CreateDraftOrder();
        var product = CreateProduct(stock: 10);

        order.AddLine(product, 4);

        Assert.Equal(10, product.StockQuantity);
    }

    [Fact]
    public void AddLine_WhenOrderIsValidated_ThrowsBusinessRuleException()
    {
        var order = CreateDraftOrder();
        var product = CreateProduct();
        order.AddLine(product, 1);
        order.Validate();

        Assert.Throws<BusinessRuleException>(() => order.AddLine(product, 1));
    }

    [Fact]
    public void Validate_WithoutLines_ThrowsBusinessRuleException()
    {
        var order = CreateDraftOrder();

        Assert.Throws<BusinessRuleException>(() => order.Validate());
    }

    [Fact]
    public void Validate_WithLines_SetsStatusToValidated()
    {
        var order = CreateDraftOrder();
        order.AddLine(CreateProduct(), 1);

        order.Validate();

        Assert.Equal(OrderStatus.VALIDATED, order.OrderStatus);
    }

    [Fact]
    public void Validate_WhenAlreadyValidated_ThrowsBusinessRuleException()
    {
        var order = CreateDraftOrder();
        order.AddLine(CreateProduct(), 1);
        order.Validate();

        Assert.Throws<BusinessRuleException>(() => order.Validate());
    }

    [Fact]
    public void Cancel_SetsStatusToCancelled_AndCannotBeCancelledTwice()
    {
        var order = CreateDraftOrder();

        order.Cancel();

        Assert.Equal(OrderStatus.CANCELLED, order.OrderStatus);
        Assert.Throws<BusinessRuleException>(() => order.Cancel());
    }

    [Fact]
    public void ClearLines_RemovesLinesAndResetsTotals()
    {
        var order = CreateDraftOrder();
        order.AddLine(CreateProduct(), 2);

        order.ClearLines();

        Assert.Empty(order.Lines);
        Assert.Equal(0m, order.TotalHt);
        Assert.Equal(0m, order.TotalTtc);
    }

    [Fact]
    public void ClearLines_WhenOrderIsValidated_ThrowsBusinessRuleException()
    {
        var order = CreateDraftOrder();
        order.AddLine(CreateProduct(), 1);
        order.Validate();

        Assert.Throws<BusinessRuleException>(() => order.ClearLines());
    }
}
