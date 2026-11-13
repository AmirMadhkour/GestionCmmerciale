using FluentValidation;
using GestionCommerciale.Application.DTOs.Clients;

namespace GestionCommerciale.Application.Validators.Clients;

public class UpdateClientValidator : AbstractValidator<UpdateClientDto>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.FirstNameOrCompany).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^\+?[0-9\s]{8,15}$");
    }
}