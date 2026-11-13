using FluentValidation;
using GestionCommerciale.Application.DTOs.Orders; 
namespace GestionCommerciale.Application.Validators.Orders;

public class OrderLineInputValidator : AbstractValidator<OrderLineInputDto>
{
    public OrderLineInputValidator()
    {
        RuleFor(x => x.ProductID)
            .NotEmpty().WithMessage("Le produit est obligatoire pour chaque ligne.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("La quantité d'une ligne doit être supérieure à 0.");
    }
}