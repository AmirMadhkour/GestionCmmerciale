
namespace GestionCommerciale.Application.DTOs.Orders;

public class OrderDetailDto
{
    public Guid Id {get; set;}
    public string OrderNumber {get; set;} = string.Empty;
    public Guid ClientId {get; set;}
    public string ClientName {get; set;} = string.Empty;
    public DateTime OrderDate {get; set;} = DateTime.UtcNow;
    public string OrderStatus {get; set;} = string.Empty;
    public decimal TotalHt {get; set;}
    public decimal TotalTTC {get; set;}

    public List<OrderLineDto> Lines {get; set;} = new();
}
