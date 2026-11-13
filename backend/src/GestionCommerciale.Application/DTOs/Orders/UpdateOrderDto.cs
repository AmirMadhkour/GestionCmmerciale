namespace GestionCommerciale.Application.DTOs.Orders;

public class UpdateOrderDto
{
    public Guid ClientId {get; set;}

    public List<OrderLineInputDto> Lines {get; set;} = new();
}