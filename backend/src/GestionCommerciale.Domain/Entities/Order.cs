using System.Net;
using GestionCommerciale.Domain.Common;
using GestionCommerciale.Domain.Constants;
using GestionCommerciale.Domain.Enums;
using GestionCommerciale.Domain.Exceptions;

namespace GestionCommerciale.Domain.Entities;

public class Order : BaseEntity
{
    
    public string? OrderNumber {get; private set;} 

    public Guid ClientId {get; set;}
    public Client Client {get; set;} = null! ; 

    public DateTime DateOrder {get; set;} =  DateTime.UtcNow ;

    public OrderStatus OrderStatus {get; private set;} = OrderStatus.DRAFT;

    private readonly List<OrderLine> _lines = new(); 

    public IReadOnlyCollection <OrderLine> Lines => _lines.AsReadOnly(); 

    public decimal TotalHt {get; private set; } 

    public decimal TotalTtc {get; private set;}

    private Order () {}

    public static Order  Create ( Guid clientId)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            DateOrder = DateTime.UtcNow,
            OrderStatus = OrderStatus.DRAFT
        } ;
    }

    public void SetOrderNumber ( string orderNumber)
    {
        OrderNumber = orderNumber;
    }
    
    public void AddLine (
        Product product,
        int Q
    )
    {
        if (OrderStatus != OrderStatus.DRAFT)
            throw new BusinessRuleException("Impossible d'ajouter des lignes à une commande qui n'est pas en brouillon.");
        if (Q <= 0 )
            throw new BusinessRuleException(" Quantité doit être supérieur a 0");
        if ( product.UnitPriceHT < 0 )
        throw new BusinessRuleException("Prix négatif , Impossible");

        var existingLine = _lines.FirstOrDefault(l => l.ProductId == product.Id);
        if (existingLine != null)
        {
            existingLine.UpdateQuantity(existingLine.Quantity + Q);
        }
        else
        {
            _lines.Add(OrderLine.Create(Id, product, Q));
        }
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        TotalHt = _lines.Sum(l => l.LineTotal);
        TotalTtc = TotalHt * ( 1 + TaxConstants.VATRATE);
    }
    public void Validate()
    {
        
        if (OrderStatus != OrderStatus.DRAFT )
            throw new BusinessRuleException ("Seule une commende Draft peut etre validé");
        
        if (!_lines.Any())
            throw new BusinessRuleException("Impossible de validé une commennde vide ");
        
        OrderStatus = OrderStatus.VALIDATED ; 
    }

    public void  ClearLines()
    {
        if ( OrderStatus != OrderStatus.DRAFT)
            throw new BusinessRuleException ("Impossible de modifié une ligne n'est pas en Brouillon");
        _lines.Clear();
        RecalculateTotals();
    }

    public void RemoveLine(Guid productId)
    {
        if (OrderStatus != OrderStatus.DRAFT)
            throw new InvalidOperationException("Impossible de modifier une commande déjà validée ou annulée.");

        var line = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (line != null)
        {
            _lines.Remove(line);
            RecalculateTotals();
        }
    }
    public void Cancel()
    {
        if (OrderStatus == OrderStatus.CANCELLED)
            throw new BusinessRuleException (" Commende deja annulée");
        OrderStatus = OrderStatus.CANCELLED;
    }
}