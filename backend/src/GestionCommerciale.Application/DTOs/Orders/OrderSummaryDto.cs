namespace GestionCommerciale.Application.DTOs.Orders;

public class OrderSummaryDto
{
    public Guid Id {get; set;}
    public string OrderNumber {get; set;} = string.Empty;
    public string ClientName  {get; set;} = string.Empty; 

    public DateTime OrderDate {get; set;} 
    public string Status {get; set;} = string.Empty;
    public decimal TotalHT {get; set;}
    public decimal TotalTTC {get; set;}
}