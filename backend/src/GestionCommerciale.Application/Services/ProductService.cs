using GestionCommerciale.Application.Common.Exceptions;
using GestionCommerciale.Application.DTOs.Products;
using GestionCommerciale.Application.Interfaces.Repositories;
using GestionCommerciale.Application.Interfaces.Services;
using GestionCommerciale.Application.Mappings;
using GestionCommerciale.Domain.Entities;

namespace GestionCommerciale.Application.Services;

public class ProductService  : IProductService
{
    private readonly IProductRepository _productRepository ;
    private readonly IUnitOfWork _unitOfWork ;

    public ProductService ( IProductRepository productRepository , IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork ;
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new NotFoundException($"produit {id} notfound");
        
        return product.ToDto();
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync ()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => p.ToDto()).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        if (await _productRepository.ReferenceExistsAsync(dto.Reference))
            throw new ConflictException("produit existe deja");
        var product = Product.Create(dto.Reference , dto.Name , dto.Description , dto.UnitPriceHT , dto.InitialStock);
        await _productRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product.ToDto();
    }

    public async Task UpdateAsync(Guid id , UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) 
            throw new NotFoundException("product not  found");
        if (product.Reference != dto.Reference && await _productRepository.ReferenceExistsAsync(dto.Reference) )
            throw new ConflictException("ref exist ! ");
        product.UpdateInfo(dto.Reference, dto.Name, dto.Description, dto.UnitPriceHT);
        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync (Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) 
            throw new NotFoundException("product not  found");

        if (await _productRepository.IsUsedInOrdersAsync(id))
            throw new ConflictException("Impossible de supprimer un produit utilisé dans des commandes.");

        _productRepository.Remove(product);
        await _unitOfWork.SaveChangesAsync();
    }
}