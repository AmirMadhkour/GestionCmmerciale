using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderWithDetailsAsync(Guid id);
    Task<IReadOnlyList<Order>> GetByClientIdAsync(Guid clientId);
    Task<string> GenerateOrderNumberAsync ();
}