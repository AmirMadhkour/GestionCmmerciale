using GestionCommerciale.Application.DTOs.Orders;

namespace GestionCommerciale.Application.Interfaces.Services;


public interface IOrderService
{
    Task<OrderDetailDto> GetByIdAsync (Guid id);
    Task<OrderDetailDto> CreateAsync (CreateOrderDto dto);
    Task<IReadOnlyCollection<OrderSummaryDto>> GetByClientIdAsync( Guid clientId);
    Task<IReadOnlyCollection<OrderSummaryDto>> GetAllAsync ();
    Task UpdateAsync (Guid id , UpdateOrderDto dto);
    Task DeleteAsync (Guid id);

    Task ValidateAsync ( Guid id);
    Task CancelAsync ( Guid id);
}