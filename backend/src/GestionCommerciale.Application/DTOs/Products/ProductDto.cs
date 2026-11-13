namespace GestionCommerciale.Application.DTOs.Products;

public class ProductDto
{
    public Guid Id {get ; set;}
    public string Reference {get ; set;} = string.Empty;
    public string Name {get ; set;} = string.Empty;
    public string Description {get ; set;} = string.Empty; 
    public decimal UnitPriceHT {get ; set;}
    public int  StockQuantity  {get ; set;}
}