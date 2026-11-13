using FluentValidation;
using GestionCommerciale.Application.DTOs.Orders;

namespace GestionCommerciale.Application.Validators.Orders;

public class UpdateOrderValidator : AbstractValidator<UpdateOrderDto>
{
    public UpdateOrderValidator()
    {
        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("La commande doit contenir au moins une ligne.");

        RuleForEach(x => x.Lines).SetValidator(new OrderLineInputValidator());
    }
}