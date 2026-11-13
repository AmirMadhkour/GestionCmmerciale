using GestionCommerciale.Domain.Common;

namespace GestionCommerciale.Domain.Entities;

public class OrderLine : BaseEntity
{
    
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; } 
    public decimal LineTotal { get; private set; }

   
    private OrderLine() { }

    
    public static OrderLine Create(Guid orderId, Product product, int quantity)
    {
        return new OrderLine
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = product.Id,
            Product = product, 
            Quantity = quantity,
            UnitPrice = product.UnitPriceHT, 
            LineTotal = quantity * product.UnitPriceHT
        };
    }

  
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("La quantité doit être supérieure à zéro.");

        Quantity = newQuantity;
        
        LineTotal = Quantity * UnitPrice; 
    }
}