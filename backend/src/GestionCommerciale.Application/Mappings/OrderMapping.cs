using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using GestionCommerciale.Application.DTOs.Orders;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Mappings;


public static class  OrderMapping  {
    public static OrderSummaryDto ToSummaryDto ( this Order Order)
    {
        return new OrderSummaryDto
        {
            Id = Order.Id ,
            OrderNumber = Order.OrderNumber ?? string.Empty,
            ClientName = Order.Client != null ? $"{Order.Client.LastName} {Order.Client.FirstNameOrCompany}".Trim() : string.Empty,
            OrderDate = Order.DateOrder,
            Status = Order.OrderStatus.ToString(),
            TotalHT = Order.TotalHt,
            TotalTTC = Order.TotalTtc
        };
    }

    public static OrderDetailDto ToDetailDto( this Order Order)
    {
        return new OrderDetailDto
        {
            
            Id = Order.Id ,
            OrderNumber = Order.OrderNumber ?? string.Empty,
            ClientName = Order.Client != null ? $"{Order.Client.LastName} {Order.Client.FirstNameOrCompany}".Trim() : string.Empty,
            OrderDate = Order.DateOrder,
            ClientId = Order.ClientId,
            OrderStatus = Order.OrderStatus.ToString(),
            TotalHt = Order.TotalHt,
            TotalTTC = Order.TotalTtc,
            Lines = Order.Lines.Select(line => line.ToDto()).ToList()
        };
    }

    public static OrderLineDto ToDto(this OrderLine line)
    {
        return new OrderLineDto
        {
            ProductId = line.ProductId,
            ProductName = line.Product != null ? line.Product.ProductName : string.Empty,
            Quantity = line.Quantity,
            UnitPrice = line.UnitPrice,
            LineTotal = line.LineTotal
        };
    }
}