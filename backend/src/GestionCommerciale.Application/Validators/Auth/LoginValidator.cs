using GestionCommerciale.Application.DTOs.Auth;
using FluentValidation;

namespace GestionCommerciale.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x=> x.Email)
            .NotEmpty().WithMessage("Email requis")
            .EmailAddress().WithMessage("le format de l'email est invalide");

        RuleFor(x=>x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis");
    }
}