using GestionCommerciale.Application.DTOs.Products;

namespace GestionCommerciale.Application.Interfaces.Services;

public interface IProductService
{
    Task<ProductDto> GetByIdAsync (Guid id);
    Task<IReadOnlyCollection<ProductDto>> GetAllAsync();
    Task<ProductDto> CreateAsync (CreateProductDto dto);
    Task UpdateAsync (Guid id , UpdateProductDto dto);
    Task DeleteAsync (Guid id);

}