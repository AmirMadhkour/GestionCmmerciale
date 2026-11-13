using FluentValidation;
using GestionCommerciale.Application.DTOs.Products;

namespace GestionCommerciale.Application.Validators.Products;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("La référence est obligatoire.")
            .MaximumLength(50).WithMessage("La référence ne doit pas dépasser 50 caractères.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du produit est obligatoire.")
            .MaximumLength(200).WithMessage("Le nom est trop long.");

        RuleFor(x => x.UnitPriceHT)
            .GreaterThanOrEqualTo(0).WithMessage("Le prix ne peut pas être négatif.");

        RuleFor(x => x.InitialStock)
            .GreaterThanOrEqualTo(0).WithMessage("Le stock initial ne peut pas être négatif.");
    }
}