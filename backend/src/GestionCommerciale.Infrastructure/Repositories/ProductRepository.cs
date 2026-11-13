using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public async Task<bool> ReferenceExistsAsync(string reference)
    {
        return await _dbSet.AnyAsync(p => p.Reference == reference);
    }

    public async Task<bool> IsUsedInOrdersAsync(Guid productId)
    {
        return await _context.OrderLines.AnyAsync(l => l.ProductId == productId);
    }
}