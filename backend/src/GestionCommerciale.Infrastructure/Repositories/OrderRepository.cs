using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public async Task<Order?> GetOrderWithDetailsAsync(Guid id)
    {
      return await _dbSet.Include(o=>o.Client)
                        .Include(o=>o.Lines)
                        .ThenInclude(l=>l.Product)
                        .FirstOrDefaultAsync(o=>o.Id == id);
    }

    public override async Task<IReadOnlyList<Order>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking()
                           .Include(o => o.Client)
                           .OrderByDescending(o => o.DateOrder)
                           .ToListAsync();
    }

    public async Task<IReadOnlyList<Order>> GetByClientIdAsync(Guid clientId)
    {
        return await _dbSet.AsNoTracking()
                           .Include(o => o.Client)
                           .Where(o => o.ClientId == clientId)
                           .OrderByDescending(o => o.DateOrder)
                           .ToListAsync();
    }

   public async Task<string> GenerateOrderNumberAsync()
    {
        var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
        var prefix = $"CMD-{currentMonth}-";

        var lastOrder = await _dbSet
            .Where(o => o.OrderNumber != null && o.OrderNumber.StartsWith(prefix))
            .OrderByDescending(o => o.OrderNumber) 
            .FirstOrDefaultAsync();

        if (lastOrder == null || string.IsNullOrWhiteSpace(lastOrder.OrderNumber))
        {
            return $"{prefix}0001";
        }

        var lastNumberStr = lastOrder.OrderNumber.Substring(prefix.Length);
        
        if (int.TryParse(lastNumberStr, out int lastNumber))
        {
            return $"{prefix}{(lastNumber + 1):D4}"; 
        }

        return $"{prefix}0001";
    }
}