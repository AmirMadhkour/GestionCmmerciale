using GestionCommerciale.Domain.Common;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.Domain.Entities;

public class Product : BaseEntity
{
    //private constructr
    private Product() { }  
    public string Reference {get; private set;} = string.Empty;

    public string ProductName {get ; private set;} = string.Empty;

    public string Description {get; private set;} = string.Empty;

    public decimal UnitPriceHT {get; private set;} 

    public int StockQuantity {get;private set;}



    public static Product Create (
        string refe,
        string name ,
        string desc ,
        decimal unitPriceHt,
        int initialStock )
    {
        if (string.IsNullOrWhiteSpace(refe)) 
            throw new BusinessRuleException("Produit doit être référencé");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("Produit doit avoir un nom");   

        if(unitPriceHt < 0 )
            throw new BusinessRuleException("Prix doit être positif");
        
        var product = new Product {
        Reference = refe ,
        ProductName = name ,
        Description = desc ,
        UnitPriceHT = unitPriceHt,
        };
        product.SetInitialStock(initialStock);
        
        return product ; 
    }
    
    public void SetInitialStock ( int quantity)
    {
        if (quantity < 0) throw new BusinessRuleException("Le Stock initial ne peut pas être négatif.");
            StockQuantity = quantity ; 
    }

    public void DecreaseStock ( int quantity)
    {
        if (quantity <= 0) throw new BusinessRuleException("Quantité à réduire doit être supérieur à zéro.");
        else if (StockQuantity < quantity) throw new BusinessRuleException("Stock insufisant");
        StockQuantity -= quantity ;
    }

    public void IncreaseStock (int quantity)
    {
        if (quantity <= 0) throw new BusinessRuleException("Quantité à ajouté doit être supérieur à zéro.");
        StockQuantity += quantity ;
    }

    public void UpdateInfo(string refe, string name, string desc, decimal unitPriceHt)
    {
       
        if (string.IsNullOrWhiteSpace(refe)) 
            throw new BusinessRuleException("La référence du produit ne peut pas être vide.");

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleException("Le nom du produit ne peut pas être vide.");   

        if (unitPriceHt < 0)
            throw new BusinessRuleException("Le prix doit être positif.");

        
        Reference = refe.Trim();
        ProductName = name.Trim();
        Description = desc?.Trim() ?? string.Empty;
        UnitPriceHT = unitPriceHt;
    }

}