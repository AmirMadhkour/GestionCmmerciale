using GestionCommerciale.Application.DTOs.Products;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Mappings;

public static class ProductMapping
{
    public static ProductDto ToDto( this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Reference= product.Reference,
            Name = product.ProductName,
            Description= product.Description,
            UnitPriceHT= product.UnitPriceHT,
            StockQuantity= product.StockQuantity
        };
    }
}