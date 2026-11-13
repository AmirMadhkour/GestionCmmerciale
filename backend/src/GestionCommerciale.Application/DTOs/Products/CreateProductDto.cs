namespace GestionCommerciale.Application.DTOs.Products;

public class CreateProductDto
{
    
    public string Reference {get ; set;} = string.Empty;
    public string Name {get ; set;} = string.Empty;
    public string Description {get ; set;} = string.Empty; 
    public decimal UnitPriceHT {get ; set;}
    public int  InitialStock  {get ; set;}
}