namespace GestionCommerciale.Application.DTOs.Orders;

public class CreateOrderDto
{
    public Guid ClientId {get; set;}

    public List <OrderLineInputDto> Lines {get; set;} = new();
}