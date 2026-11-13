using FluentValidation;
using GestionCommerciale.Application.DTOs.Products;

namespace GestionCommerciale.Application.Validators.Products;

public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.UnitPriceHT).GreaterThanOrEqualTo(0);
       
    }
}