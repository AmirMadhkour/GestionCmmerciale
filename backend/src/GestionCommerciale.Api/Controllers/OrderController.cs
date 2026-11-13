using GestionCommerciale.Application.DTOs.Orders;
using GestionCommerciale.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionCommerciale.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] 
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetAll()
    {
        return Ok(await _orderService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto>> GetById(Guid id)
    {
        return Ok(await _orderService.GetByIdAsync(id));
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<IReadOnlyCollection<OrderSummaryDto>>> GetByClient(Guid clientId)
    {
        return Ok(await _orderService.GetByClientIdAsync(clientId));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetailDto>> Create(CreateOrderDto dto)
    {
        var order = await _orderService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateOrderDto dto)
    {
        await _orderService.UpdateAsync(id, dto);
        return NoContent();
    }

    
    [HttpPost("{id}/validate")]
    [Authorize(Roles = "ADMIN")] 
    public async Task<ActionResult> Validate(Guid id)
    {
        await _orderService.ValidateAsync(id);
        return Ok(new { Message = "Commande validée avec succès, les stocks ont été mis à jour." });
    }

    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult> Cancel(Guid id)
    {
        await _orderService.CancelAsync(id);
        return Ok(new { Message = "Commande annulée." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")] 
    public async Task<ActionResult> Delete(Guid id)
    {
        await _orderService.DeleteAsync(id);
        return NoContent();
    }
}