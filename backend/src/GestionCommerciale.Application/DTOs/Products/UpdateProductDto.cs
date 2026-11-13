namespace GestionCommerciale.Application.DTOs.Products;

public class UpdateProductDto
{
    public string Reference {get ; set;} = string.Empty;
    public string Name {get ; set;} = string.Empty;
    public string Description {get ; set;} = string.Empty; 
    public decimal UnitPriceHT {get ; set;}
    
}