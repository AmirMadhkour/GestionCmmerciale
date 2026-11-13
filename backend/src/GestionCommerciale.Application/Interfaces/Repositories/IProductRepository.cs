using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<bool> ReferenceExistsAsync(string Reference);
    Task<bool> IsUsedInOrdersAsync(Guid productId);
}