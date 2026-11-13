namespace GestionCommerciale.Application.DTOs.Orders;

public class OrderLineInputDto
{
    public Guid ProductID {get; set;}
    public int Quantity {get; set;}
}