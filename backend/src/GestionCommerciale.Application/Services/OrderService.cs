using System.Security.Cryptography.X509Certificates;
using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Orders;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Interfaces.Services;
using GestionCommerciale.Application.Mappings;
using GestionCommerciale.Domain.Entities;
using GestionCommerciale.Domain.Enums;


namespace GestionCommerciale.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository ;
    private readonly IUnitOfWork _unitOfWork ;
    private readonly IClientRepository _clientRepository;
    private readonly IProductRepository _productRepository;

    public OrderService ( IOrderRepository orderRepository  ,IUnitOfWork unitOfWork, IClientRepository clientRepository, IProductRepository productRepository)
    {
        _orderRepository =orderRepository ;
        _unitOfWork = unitOfWork ;
        _clientRepository = clientRepository;
        _productRepository = productRepository;
    }

    public async Task<OrderDetailDto> GetByIdAsync (Guid id)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(id) ; 
        if ( order == null) 
            throw new NotFoundException("order introuvable");

        return order.ToDetailDto();
    }

    public async Task<IReadOnlyCollection<OrderSummaryDto>> GetAllAsync ( )
    {
        var orders = await  _orderRepository.GetAllAsync();
        return orders.Select(o => o.ToSummaryDto()).ToList();
    }

    public async Task<IReadOnlyCollection<OrderSummaryDto>> GetByClientIdAsync(Guid id)
    {
        var orders = await _orderRepository.GetByClientIdAsync(id);
        return orders.Select(o => o.ToSummaryDto()).ToList();
    }

    public async Task<OrderDetailDto> CreateAsync(CreateOrderDto dto)
    {
        var client = await _clientRepository.GetByIdAsync(dto.ClientId);
        if (client == null)
            throw new NotFoundException("client not found");
        var order = Order.Create(client.Id);
        order.Client = client;
        order.SetOrderNumber(await _orderRepository.GenerateOrderNumberAsync());

        foreach( var lineDto in dto.Lines)
        {
            var product = await _productRepository.GetByIdAsync(lineDto.ProductID);
            if ( product == null) 
                throw new NotFoundException($"produit {lineDto.ProductID} introuvable");
            
            order.AddLine(product, lineDto.Quantity);
        }
        await _orderRepository.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();
        return order.ToDetailDto();
    }

    public async Task UpdateAsync (Guid id , UpdateOrderDto dto)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(id);
        if (order == null)
            throw new NotFoundException("order not found");
        order.ClearLines();

        foreach( var lineDto in dto.Lines)
        {
            var product = await _productRepository.GetByIdAsync(lineDto.ProductID);
            if (product == null)
                throw new NotFoundException($"produit {lineDto.ProductID} introuvable");
            order.AddLine(product, lineDto.Quantity );

        }
        // L'order est déjà suivi par EF : pas de Update() (il marquerait les nouvelles lignes comme Modified)
        await _unitOfWork.SaveChangesAsync();

    }

    public async Task DeleteAsync (Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
         if (order == null)
            throw new NotFoundException("order not found");
        
        _orderRepository.Remove(order);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ValidateAsync ( Guid id )
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(id);
        if ( order == null)
            throw new NotFoundException("commende introuvé");
        
        order.Validate();

        foreach ( var line in order.Lines)
        {
            var product  = await _productRepository.GetByIdAsync(line.ProductId);
            if (product != null)
            {
                product.DecreaseStock(line.Quantity);
                _productRepository.Update(product);
            }
        }
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync();
        
    }

    public async Task CancelAsync ( Guid id)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(id);
        if ( order == null)
            throw new NotFoundException("commende introuvé");
        
        // Le stock n'est décrémenté qu'à la validation : on ne le restitue que pour une commande validée
        var stockWasDeducted = order.OrderStatus == OrderStatus.VALIDATED;

        order.Cancel();

        if (stockWasDeducted)
        {
            foreach ( var line in order.Lines)
            {
                var product  = await _productRepository.GetByIdAsync(line.ProductId);
                product?.IncreaseStock(line.Quantity);
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }

}