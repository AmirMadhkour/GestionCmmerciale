using FluentValidation;
using GestionCommerciale.Application.DTOs.Orders;

namespace GestionCommerciale.Application.Validators.Orders;

public class CreateOrderValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("La commande doit être rattachée à un client.");

        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("La commande doit contenir au moins une ligne de produit.");

        
        RuleForEach(x => x.Lines).SetValidator(new OrderLineInputValidator());
    }
}